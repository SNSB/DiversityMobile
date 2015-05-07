extern alias Model;

namespace DiversityPhone.Helper
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using DiversityPhone.Model.Legacy;
    using DiversityPhone.Services;
    using Ninject;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;

    public static partial class VersionMigration
    {
        private const string LAST_VERSION = "LastVersion";

        private static Version GetCurrentVersionNumber()
        {
            var asm = Assembly.GetExecutingAssembly();
            var parts = asm.FullName.Split(',');
            return new Version(parts[1].Split('=')[1]);
        }

        public static Task ApplyMigrationIfNecessary()
        {
            return Task.Factory.StartNew(async () =>
            {
                var currentProfile = App.Kernel.Get<ICurrentProfile>().CurrentProfilePath();

                Version currentVersion = GetCurrentVersionNumber();
                Version lastVersion;
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(LAST_VERSION, out lastVersion))
                {
                    // Version before we had LAST_VERSION
                    // Versions up to and including 0.9.9
                    MigrateSettingsFromApplicationSettings();

                    await MoveDatabaseToProfile(currentProfile);
                    MoveVocabularyToProfile(currentProfile);
                    MoveMultimediaToProfile(currentProfile);

                    await CreateOrUpgradeSchema();

                    lastVersion = new Version(0, 9, 10, 0);
                }
                if (lastVersion < currentVersion)
                {
                    await CreateOrUpgradeSchema();

                    if (lastVersion < new Version(0, 9, 10, 0))
                    {
                        var dbLocation = Path.Combine(currentProfile, DiversityDataContext.DB_FILENAME);
                        MoveGeoPointsToLocalizations(new DiversityDataContext(dbLocation));
                    }
                    if (lastVersion < new Version(1, 0, 1, 0))
                    {
                        // Select all downloaded taxon lists
                        var taxonService = App.Kernel.Get<ITaxonService>();
                        var unselected = from list in taxonService.getTaxonLists()
                                         where !list.IsSelected
                                         select list;

                        foreach (var list in unselected)
                        {
                            list.IsSelected = true;
                            taxonService.updateTaxonList(list);
                        }
                    }
                }

                if (IsolatedStorageSettings.ApplicationSettings.Contains(LAST_VERSION))
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove(LAST_VERSION);
                }
                IsolatedStorageSettings.ApplicationSettings.Add(LAST_VERSION, currentVersion);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }).Unwrap();
        }

        private static void MoveGeoPointsToLocalizations(DiversityDataContext src, DiversityDataContext dst = null, IEnumerable<Action<GeoPointForSeries>> mappings = null)
        {
            dst = dst ?? src;
            mappings = mappings ?? Enumerable.Empty<Action<GeoPointForSeries>>();

            try
            {
                var pointsTable = src.GetTable<GeoPointForSeries>();

                if (pointsTable.Any())
                {
                    var locTable = dst.GetTable<Localization>();

                    var locs = pointsTable
                        .AsEnumerable()
                        // Apply mappings
                        .Select(p =>
                        {
                            foreach (var f in mappings)
                            {
                                f(p);
                            }
                            return p;
                        })
                        .Select(point =>
                        new Localization()
                        {
                            RelatedID = point.SeriesID,
                            Altitude = point.Altitude,
                            Longitude = point.Longitude,
                            Latitude = point.Latitude,
                            ModificationState = point.ModificationState
                        });

                    locTable.InsertAllOnSubmit(locs);
                    pointsTable.DeleteAllOnSubmit(pointsTable);

                    dst.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(VersionMigration)).ErrorException("Migrating GeoPoints", ex);
            }
        }

        private static void MoveVocabularyToProfile(string currentProfile)
        {
            var sourceLocation = "vocabularyDB.sdf";
            var targetLocation = Path.Combine(currentProfile, "VocabularyDB.sdf");

            MoveFileIfExists(sourceLocation, targetLocation);
        }

        /// <summary>
        /// Moves a FieldData DB File from its location in older versions (root folder)
        /// into the Profile Folder (Added in Version 0.9.9)
        /// Note: Checks, if the PrimaryKey Columns in the DB have been marked with IsDbGenerated
        /// as introduced in Version 0.9.7
        /// if not, then the DB is copied over Entity by Entity into a newly created Instance
        /// </summary>
        /// <param name="currentProfile"></param>
        private static async Task MoveDatabaseToProfile(string currentProfile)
        {
            var sourceLocation = "diversityDB.sdf";
            var targetLocation = Path.Combine(currentProfile, DiversityDataContext.DB_FILENAME);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(sourceLocation))
                {
                    if (iso.FileExists(targetLocation))
                    {
                        iso.DeleteFile(targetLocation);
                    }

                    if (!iso.DirectoryExists("Temp"))
                    {
                        iso.CreateDirectory("Temp");
                    }

                    var tempTargetLocation = string.Format("Temp/{0}", DiversityDataContext.DB_FILENAME);

                    iso.CopyFile(sourceLocation, tempTargetLocation);

                    var FileCopyMigration = false;
                    // If the key columns are NOT DbGenerated, then insert with new Classes will fail.
                    using (var ctx = new DiversityDataContext(tempTargetLocation))
                    {
                        var es = new EventSeries()
                        {
                            CollectionSeriesID = null,
                            Description = "TestD",
                            ModificationState = ModificationState.Unmodified,
                            SeriesCode = "SomeCode",
                            SeriesEnd = null,
                            SeriesID = 0,
                            SeriesStart = DateTime.Now
                        };

                        try
                        {
                            ctx.EventSeries.InsertOnSubmit(es);
                            ctx.SubmitChanges();
                            FileCopyMigration = true;
                        }
                        catch (Exception)
                        {
                            FileCopyMigration = false;
                        }
                    }

                    if (FileCopyMigration)
                    {
                        iso.CopyFile(sourceLocation, targetLocation);
                    }
                    else
                    {
                        // We have to at least partially upgrade the source DB before migration
                        // Don't use the source db for that but a throwaway copy
                        var tmpSourceLocation = Path.ChangeExtension(targetLocation, ".old.sdf");
                        iso.CopyFile(sourceLocation, tmpSourceLocation);
                        await CreateOrUpgradeSchema(tmpSourceLocation, new Version(0, 9, 8));

                        // Create target DB using the most recent schema
                        await CreateOrUpgradeSchema(targetLocation);

                        using (var sourceCtx = new DiversityDataContext(tmpSourceLocation))
                        using (var targetCtx = new DiversityDataContext(targetLocation))
                        {
                            CopyDatabase(sourceCtx, targetCtx);
                        }

                        //cleanup
                        iso.DeleteFile(tmpSourceLocation);

                        // Move old file into Profile dir so it gets picked up for import/export
                        iso.MoveFile(sourceLocation, tmpSourceLocation);
                    }
                }
            }
        }

        private static void CopyDatabase(DiversityDataContext sourceCtx, DiversityDataContext targetCtx)
        {
            // EventSeries
            var esMap = CopyTableAndCreateMapping<EventSeries>(sourceCtx, targetCtx, x => x.SeriesID, new Action<EventSeries>[0]);

            // GeoPoints
            var geopointMappings = new Action<GeoPointForSeries>[] { (g) => { g.SeriesID = esMap[g.SeriesID]; } };
            MoveGeoPointsToLocalizations(sourceCtx, targetCtx, geopointMappings);

            //Events
            var evMappings = new Action<Event>[] { (g) => {
                if(g.SeriesID.HasValue) {
                    g.SeriesID = esMap[g.SeriesID.Value];
                }
            }};
            var evMap = CopyTableAndCreateMapping<Event>(sourceCtx, targetCtx, x => x.EventID, evMappings);

            // Event Properties
            var propMappings = new Action<EventProperty>[] { (g) => { g.EventID = evMap[g.EventID]; } };
            CopyTableWithMappings<EventProperty>(sourceCtx, targetCtx, propMappings);

            // Specimen
            var spMapping = new Action<Specimen>[] { (g) => { g.EventID = evMap[g.EventID]; } };
            var spMap = CopyTableAndCreateMapping<Specimen>(sourceCtx, targetCtx, x => x.SpecimenID, spMapping);

            // IUs
            var iuMappings = new Action<IdentificationUnit>[] { (g) => { g.SpecimenID = spMap[g.SpecimenID]; } };
            var iuMap = CopyTableAndCreateMapping<IdentificationUnit>(sourceCtx, targetCtx, x => x.UnitID, iuMappings);
            RemapColumn<IdentificationUnit>(targetCtx, iuMap, x => x.RelatedUnitID, (x, k) => x.RelatedUnitID = k);

            // IUAN
            var iuanMappings = new Action<IdentificationUnitAnalysis>[] { (g) => { g.UnitID = iuMap[g.UnitID]; } };
            CopyTableWithMappings(sourceCtx, targetCtx, iuanMappings);

            // MMO
            var mmoMappings = new Action<MultimediaObject>[] {
                (m) => {
                    var map = esMap;
                    switch(m.OwnerType) {
                        case DBObjectType.EventSeries:
                            map = esMap;
                        break;

                        case DBObjectType.Event:
                            map = evMap;
                            break;

                        case DBObjectType.IdentificationUnit:
                            map = iuMap;
                            break;

                        case DBObjectType.Specimen:
                            map = spMap;
                            break;
                    }
                    m.RelatedId = map[m.RelatedId];
                }
            };
            CopyTableWithMappings(sourceCtx, targetCtx, mmoMappings);

            // Set Created Times of MMOs to a sensible value if necessary
            var mmos = targetCtx.GetTable<MultimediaObject>();
            var untimed = from mmo in mmos
                          where mmo.TimeStamp == null
                          select mmo;
            var now = DateTime.Now;
            foreach (var mmo in untimed)
            {
                mmo.TimeStamp = now;
            }
            targetCtx.SubmitChanges();
        }

        private static void CopyTableWithMappings<T>(DiversityDataContext sourceCtx, DiversityDataContext targetCtx, Action<T>[] mappings) where T : class
        {
            var source = sourceCtx.GetTable<T>().AsEnumerable();
            var mapped = source.Select(x =>
            {
                foreach (var mapping in mappings)
                {
                    mapping(x);
                }
                return x;
            });

            targetCtx.GetTable<T>().InsertAllOnSubmit(mapped);
            targetCtx.SubmitChanges();
        }

        private static IDictionary<int, int> CopyTableAndCreateMapping<T>(DiversityDataContext sourceCtx, DiversityDataContext targetCtx, Func<T, int> keySelector, Action<T>[] mappings) where T : class
        {
            var sourceList = sourceCtx.GetTable<T>().ToList();
            var idList = sourceList.Select(keySelector).ToList();

            var mapped = sourceList.Select(x =>
            {
                foreach (var mapping in mappings)
                {
                    mapping(x);
                }
                return x;
            });

            targetCtx.GetTable<T>().InsertAllOnSubmit(mapped);
            targetCtx.SubmitChanges();

            var newIdList = sourceList.Select(keySelector).ToList();

            var res = new Dictionary<int, int>();
            for (var i = 0; i < idList.Count; ++i)
            {
                res.Add(idList[i], newIdList[i]);
            }
            return res;
        }

        private static void RemapColumn<T>(DiversityDataContext ctx, IDictionary<int, int> map, Func<T, int?> keySelector, Action<T, int> keySetter, Expression<Func<T, bool>> filter = null) where T : class
        {
            IQueryable<T> items = ctx.GetTable<T>();
            if (filter != null)
            {
                items = items.Where(filter);
            }
            foreach (var item in items)
            {
                var oldKey = keySelector(item);
                if (oldKey.HasValue)
                {
                    var newKey = map[oldKey.Value];
                    keySetter(item, newKey);
                }
            }
            ctx.SubmitChanges();
        }

        private static void MoveMultimediaToProfile(string currentProfile)
        {
            var targetLocation = Path.Combine(currentProfile, MultimediaStorageService.MEDIA_FOLDER);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var multimediaDir = "/multimedia/";
                if (iso.DirectoryExists(multimediaDir))
                {
                    // For Version 0.9.8
                    // Multimedia stored in /multimedia/

                    foreach (var file in iso.GetFileNames("/multimedia/*"))
                    {
                        var sourcePath = Path.Combine(multimediaDir, file);
                        var targetPath = Path.Combine(targetLocation, file);

                        iso.MoveFile(sourcePath, targetPath);
                    }
                }
                else
                {
                    // For Versions earlier than 0.9.8
                    // Multimedia stored directly in the Root Folder
                    foreach (var file in iso.GetFileNames("/*.jpg"))
                    {
                        var sourcePath = file;
                        var targetPath = Path.Combine(targetLocation, file);

                        iso.MoveFile(sourcePath, targetPath);
                    }

                    foreach (var file in iso.GetFileNames("/*.mp4"))
                    {
                        var sourcePath = file;
                        var targetPath = Path.Combine(targetLocation, file);

                        iso.MoveFile(sourcePath, targetPath);
                    }

                    foreach (var file in iso.GetFileNames("/*.wav"))
                    {
                        var sourcePath = file;
                        var targetPath = Path.Combine(targetLocation, file);

                        iso.MoveFile(sourcePath, targetPath);
                    }
                }

                try
                {
                    // Fix up Database paths
                    var dbPath = Path.Combine(currentProfile, DiversityDataContext.DB_FILENAME);
                    var mmFolder = Path.Combine(currentProfile, MultimediaStorageService.MEDIA_FOLDER);
                    using (var ctx = new DiversityDataContext(dbPath))
                    {
                        foreach (var mmo in ctx.MultimediaObjects)
                        {
                            var uri = mmo.Uri;
                            if (!(uri.StartsWith("isostore:") || uri.StartsWith("cameraroll:")))
                            {
                                var filename = Path.GetFileName(uri);
                                var expectedPath = Path.Combine(mmFolder, filename);
                                if (iso.FileExists(expectedPath))
                                {
                                    mmo.Uri = string.Format("isostore:{0}", filename);
                                }
                            }
                        }
                        ctx.SubmitChanges();
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Log(0, "", ex.Message);
                }
            }
        }

        private static void MoveFileIfExists(string sourceLocation, string targetLocation)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(sourceLocation))
                {
                    iso.MoveFile(sourceLocation, targetLocation);
                }
            }
        }

        private static void MigrateSettingsFromApplicationSettings()
        {
            var svc = App.Kernel.Get<ISettingsService>() as SettingsService;
            var isoSettings = IsolatedStorageSettings.ApplicationSettings;

            if (isoSettings.Contains("Settings"))
            {
#pragma warning disable 0436
                // AppSettings in DiversityPhone Assembly (before 0.9.8)
                try
                {
                    AppSettings settings;
                    if (isoSettings.TryGetValue<AppSettings>("Settings", out settings))
                    {
                        svc.SaveSettings(AppSettingsMixin.ToSettings(settings));
                        IsolatedStorageSettings.ApplicationSettings.Remove("Settings");
                        IsolatedStorageSettings.ApplicationSettings.Save();
                    }
                }
                catch (InvalidCastException) { }
#pragma warning restore 0436

                // Appsettings in Model Assembly (in 0.9.8)
                Model::DiversityPhone.Model.AppSettings settings2;
                if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<Model::DiversityPhone.Model.AppSettings>("Settings", out settings2))
                {
                    svc.SaveSettings(Model::DiversityPhone.Model.AppSettingsMixin.ToSettings(settings2));
                    IsolatedStorageSettings.ApplicationSettings.Remove("Settings");
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }
            else
            {
                // Appsettings in Model Assembly
                // Stored in the Profile Folder
                // (in 0.9.9)
                var settingsPath = svc.GetSettingsPath();

                try
                {
                    using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (iso.FileExists(settingsPath))
                        {
                            using (var settingsFile = iso.OpenFile(settingsPath, FileMode.Open, FileAccess.Read))
                            {
                                svc.SaveSettings(LoadLegacySettings(settingsFile));
                            }
                        }
                    }
                }
                catch (Exception) { /*Ah well*/ }
            }
        }

        public static Settings LoadLegacySettings(IsolatedStorageFileStream settingsFile)
        {
            Model::DiversityPhone.Model.AppSettings settings2;
            var SettingsSerializer = new XmlSerializer(typeof(Model::DiversityPhone.Model.AppSettings));

            var settingsXml = XmlReader.Create(settingsFile);
            if (SettingsSerializer.CanDeserialize(settingsXml))
            {
                settings2 = (Model::DiversityPhone.Model.AppSettings)SettingsSerializer.Deserialize(settingsXml);

                return Model::DiversityPhone.Model.AppSettingsMixin.ToSettings(settings2);
            }
            return null;
        }
    }
}