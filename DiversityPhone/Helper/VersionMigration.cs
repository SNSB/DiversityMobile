namespace DiversityPhone.Helper {
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using DiversityPhone.Services;
    using Microsoft.Phone.Data.Linq;
    using Ninject;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class VersionMigration {
        private const string LAST_VERSION = "LastVersion";

        private static Version GetVersionNumber()
        {
            var asm = Assembly.GetExecutingAssembly();
            var parts = asm.FullName.Split(',');
            return new Version(parts[1].Split('=')[1]);
        }

        public static async Task ApplyMigrationIfNecessary() {
            var currentProfile = App.Kernel.Get<ICurrentProfile>().CurrentProfilePath();

            Version currentVersion = GetVersionNumber();
            Version lastVersion;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(LAST_VERSION, out lastVersion))
            {
                // Version before we had LAST_VERSION
                // Versions up to and including 0.9.9
                MigrateSettingsFromApplicationSettings();

                MoveDatabaseToProfile(currentProfile);
                MoveVocabularyToProfile(currentProfile);
                await MoveMultimediaToProfile(currentProfile);
            }
            else if (lastVersion < currentVersion)
            {
                // Nothing so far :)
            }

            IsolatedStorageSettings.ApplicationSettings.Add(LAST_VERSION, currentVersion);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private static void MoveVocabularyToProfile(string currentProfile) {
            var sourceLocation = "vocabularyDB.sdf";
            var targetLocation = Path.Combine(currentProfile, "VocabularyDB.sdf");

            MoveFileIfExists(sourceLocation, targetLocation);
        }

        private static void MoveDatabaseToProfile(string currentProfile) {
            var sourceLocation = "diversityDB.sdf";
            var targetLocation = Path.Combine(currentProfile, DiversityDataContext.DB_FILENAME);

            MoveFileIfExists(sourceLocation, targetLocation);
        }

        private static Task MoveMultimediaToProfile(string currentProfile) {
            
            var targetLocation = Path.Combine(currentProfile, MultimediaStorageService.MEDIA_FOLDER);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                var multimediaDir = "/multimedia/";
                if (iso.DirectoryExists(multimediaDir))
                {
                    // For Version 0.9.8
                    // Multimedia stored in /multimedia/

                    foreach (var file in iso.GetFileNames("/multimedia/*")) {
                        var sourcePath = Path.Combine(multimediaDir, file);
                        var targetPath = Path.Combine(targetLocation, file);

                        iso.MoveFile(sourcePath, targetPath);
                    }
                } else {

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
                    // Force DB Schema update
                    DatabaseMigration.CheckAndRepairDatabase();

                    // Fix up Database paths
                    var dbPath = Path.Combine(currentProfile, DiversityDataContext.DB_FILENAME);
                    var mmFolder = Path.Combine(currentProfile, MultimediaStorageService.MEDIA_FOLDER);
                    // try writing to it to see if it has an old schema
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

        private static void MoveFileIfExists(string sourceLocation, string targetLocation) {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                if (iso.FileExists(sourceLocation)) {
                    iso.MoveFile(sourceLocation, targetLocation);
                }
            }
        }

        private static void MigrateSettingsFromApplicationSettings() {
            AppSettings settings;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<AppSettings>("Settings", out settings)) {
                var svc = App.Kernel.Get<ISettingsService>();
                svc.SaveSettings(settings.ToSettings());
                IsolatedStorageSettings.ApplicationSettings.Remove("Settings");
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

    }
}
