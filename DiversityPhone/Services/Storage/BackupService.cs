namespace DiversityPhone.Services
{
    using DiversityPhone.Helper;
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using Microsoft;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public class Snapshot
    {
        public string UserName { get; set; }

        public string ProjectName { get; set; }

        public DateTime TimeTaken { get; set; }

        /// <summary>
        /// Absolute Path to the Snapshot folder in Isolated Storage
        /// </summary>
        public string FolderPath { get; set; }
    }

    public interface IBackupService
    {
        /// <summary>
        /// Creates a new Snapshot Directory from the current Application Data set
        /// </summary>
        /// <param name="Progress">Progress reports include the currently executing stage of the operation
        /// as well as a fraction of completion if supported</param>
        Task TakeSnapshot(IProgress<Tuple<BackupStage, double>> FractionProgress);

        /// <summary>
        /// Lists all the Directories in the Snapshot Directory in Isolated Storage.
        /// These Directory Paths can be used to restore the contents into the current Application Data Set.
        /// </summary>
        /// <returns>Descriptive Data for the Locally stored Snapshots</returns>
        IObservable<Snapshot> EnumerateSnapshots();

        /// <summary>
        /// Restores the Application Data contained in the given Snapshot Folder.
        /// </summary>
        /// <param name="SnapshotFolderPath">Absolute Path to the Snapshot Directory in Isolated Storage</param>
        /// <param name="Progress">Progress as a fraction (0.0 - 1.0)</param>
        Task RestoreSnapshot(string SnapshotFolder, IProgress<double> FractionProgress);

        /// <summary>
        /// Deletes the given Snapshot Directory
        /// </summary>
        /// <param name="SnapshotFolder"></param>
        Task DeleteSnapshot(string SnapshotFolder);
    }

    public enum BackupStage
    {
        AppData,
        ExternalData
    }

    public class BackupService : IBackupService
    {
        public const string SNAPSHOTS_DIRECTORY = "Snapshots";
        public const string COMPLETED_MARKER = "Completed.dat";

        private readonly ISettingsService Settings;
        private readonly IStoreImages ImageStore;
        private readonly ICurrentProfile Profile;
        private readonly IScheduler ThreadPool;
        private readonly XmlSerializer SettingsSerializer;

        public async Task DeleteSnapshot(string SnapshotFolder)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                await iso.DeleteDirectoryRecursiveAsync(SnapshotFolder);
            }
        }

        public async Task TakeSnapshot(IProgress<Tuple<BackupStage, double>> Progress)
        {
            Contract.Requires(Progress != null);

            Progress.Report(Tuple.Create(BackupStage.AppData, 0.0));

            var currentSettings = await Settings.CurrentSettings().ToTask();

            var currentProfile = Profile.CurrentProfilePath();
            var snapshotDir = GetSnapshotPath(currentSettings);

            var appDataProgress = new Progress<double>(p => Progress.Report(Tuple.Create(BackupStage.AppData, p)));

            try
            {
                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    await isoStore.CopyDirectoryAsync(currentProfile, snapshotDir, appDataProgress);

                    await SaveMultimedia(isoStore, snapshotDir, Progress);

                    SaveCompletedTimeStamp(snapshotDir, isoStore);
                }
            }
            catch (IsolatedStorageException)
            {
                //Log
                throw;
            }
        }

        private static void SaveCompletedTimeStamp(string snapshotDir, IsolatedStorageFile isoStore)
        {
            var completedMarkerPath = Path.Combine(snapshotDir, COMPLETED_MARKER);
            using (var completedFile = isoStore.CreateFile(completedMarkerPath))
            using (var writer = new StreamWriter(completedFile))
            {
                var universalNow = DateTime.Now.ToUniversalTime();
                var invariantNowString = universalNow.ToString(CultureInfo.InvariantCulture);
                writer.WriteLine(invariantNowString);

                writer.Flush();
            }
        }

        private static DateTime? LoadCompletionTimeStampIfPresent(IsolatedStorageFile Iso, string SnapshotDir)
        {
            var completionMarkerPath = Path.Combine(SnapshotDir, COMPLETED_MARKER);
            var isCompleted = Iso.FileExists(completionMarkerPath);

            if (isCompleted)
            {
                using (var completionFile = Iso.OpenFile(completionMarkerPath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(completionFile))
                {
                    var timeString = reader.ReadToEnd();
                    DateTime readTime;
                    if (DateTime.TryParse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out readTime))
                    {
                        return readTime.ToLocalTime();
                    }
                }
            }
            return null;
        }

        private async Task SaveMultimedia(IsolatedStorageFile isoStore, string snapshotDir, IProgress<Tuple<BackupStage, double>> Progress)
        {
            Progress.Report(Tuple.Create(BackupStage.ExternalData, 0.0));

            var snapshotMultimediaDir = Path.Combine(snapshotDir, MultimediaStorageService.MEDIA_FOLDER);
            var snapshotDBPath = Path.Combine(snapshotDir, DiversityDataContext.DB_FILENAME);

            using (var db = new DiversityDataContext(snapshotDBPath))
            {
                var totalCount = db.MultimediaObjects.Count();

                if (totalCount > 0)
                {
                    var reporter = new PercentageReporter<Tuple<BackupStage, double>>(
                        Progress,
                        p => Tuple.Create(BackupStage.ExternalData, p / 100.0),
                        totalCount);

                    foreach (var mm in db.MultimediaObjects)
                    {
                        var descriptor = StorageDescriptor.FromURI(mm.Uri);
                        if (descriptor.Type == StorageType.CameraRoll)
                        {
                            using (var content = ImageStore.GetMultimedia(mm.Uri))
                            {
                                var mmFilePath = Path.Combine(snapshotMultimediaDir, descriptor.FileName);
                                using (var targetFile = isoStore.CreateFile(mmFilePath))
                                {
                                    await content.CopyToAsync(targetFile);

                                    descriptor.Type = StorageType.IsolatedStorage;
                                    mm.Uri = descriptor.ToString();
                                }
                            }
                        }
                        reporter.Completed++;
                    }

                    db.SubmitChanges();
                }
            }
        }

        private IEnumerable<Snapshot> EnumerateSnapshotsSynchronous()
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var validSnapshots = from snap in isoStore.GetDirectoryNames(string.Format("{0}/*", SNAPSHOTS_DIRECTORY))
                                     let absoluteSnap = Path.Combine(SNAPSHOTS_DIRECTORY, snap)
                                     let snapshot = ValidateAndGetSnapshotForFolder(isoStore, absoluteSnap)
                                     where snapshot != null
                                     select snapshot;
                foreach (var snap in validSnapshots)
                {
                    yield return snap;
                }
            }
        }

        public IObservable<Snapshot> EnumerateSnapshots()
        {
            return EnumerateSnapshotsSynchronous().ToObservable(ThreadPool);
        }

        private Snapshot SnapshotFromSettingsAndTime(Settings Settings, DateTime TimeTaken, string SnapshotDir)
        {
            return new Snapshot()
            {
                UserName = Settings.UserName,
                ProjectName = Settings.CurrentProjectName,
                TimeTaken = TimeTaken,
                FolderPath = SnapshotDir
            };
        }

        private Snapshot ValidateAndGetSnapshotForFolder(IsolatedStorageFile Iso, string SnapshotDir)
        {
            try
            {
                DateTime? completionTimeStamp = LoadCompletionTimeStampIfPresent(Iso, SnapshotDir);

                var settingsPath = Path.Combine(SnapshotDir, SettingsService.SETTINGS_FILE);
                var settings = Settings.LoadSettingsFromFile(settingsPath);

                // Snapshots made by version 0.9.9 store their Settings differently
                if (settings == null && Iso.FileExists(settingsPath))
                {
                    using (var settingsFile = Iso.OpenFile(settingsPath, FileMode.Open, FileAccess.Read))
                    {
                        settings = VersionMigration.LoadLegacySettings(settingsFile);
                    }

                    if (settings != null)
                    {
                        Settings.SaveSettingsToFile(settingsPath, settings);
                    }
                }

                var hasDB = Iso.FileExists(Path.Combine(SnapshotDir, DiversityDataContext.DB_FILENAME));

                if (completionTimeStamp.HasValue && settings != null && hasDB)
                {
                    return SnapshotFromSettingsAndTime(settings, completionTimeStamp.Value, SnapshotDir);
                }
            }
            catch (IsolatedStorageException) { }
            return null;
        }

        public async Task RestoreSnapshot(string SnapshotPath, IProgress<double> FractionProgress)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(SnapshotPath), "Snapshot Folder Path may not be empty");

            FractionProgress.Report(0.0);
            try
            {
                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    Contract.Requires(ValidateAndGetSnapshotForFolder(isoStore, SnapshotPath) != null);

                    var restoredProfileID = Profile.CreateProfileID();
                    var restoredProfilePath = Profile.ProfilePathForID(restoredProfileID);

                    await isoStore.CopyDirectoryAsync(SnapshotPath, restoredProfilePath, FractionProgress);

                    RemoveRestoredCompletedMarker(isoStore, restoredProfilePath);

                    Profile.SetCurrentProfileID(restoredProfileID);
                }
            }
            catch (IsolatedStorageException)
            {
            }
        }

        private static void RemoveRestoredCompletedMarker(IsolatedStorageFile isoStore, string restoredProfilePath)
        {
            var restoredCompletedMarkerPath = Path.Combine(restoredProfilePath, COMPLETED_MARKER);
            isoStore.DeleteFile(restoredCompletedMarkerPath);
        }

        private string GetSnapshotPath(Settings Settings)
        {
            return string.Format("{0}/{1}-{2}", SNAPSHOTS_DIRECTORY, Settings.UserName, DateTime.Now.ToFileTimeStamp());
        }

        private static void CreateSnapshotsDirIfNecessary()
        {
            using (var IsoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!IsoStore.DirectoryExists(SNAPSHOTS_DIRECTORY))
                {
                    IsoStore.CreateDirectory(SNAPSHOTS_DIRECTORY);
                }
            }
        }

        public BackupService(
            ISettingsService Settings,
            IStoreImages ImageStore,
            ICurrentProfile Profile,
            [ThreadPool] IScheduler ThreadPool
            )
        {
            Contract.Requires(Settings != null);
            Contract.Requires(ImageStore != null);
            Contract.Requires(Profile != null);
            Contract.Requires(ThreadPool != null);

            this.Settings = Settings;
            this.ImageStore = ImageStore;
            this.Profile = Profile;
            this.ThreadPool = ThreadPool;
            this.SettingsSerializer = new XmlSerializer(typeof(Settings));
            CreateSnapshotsDirIfNecessary();
        }
    }
}