using ICSharpCode.SharpZipLib.Zip;
using Microsoft;
using Microsoft.Live;
using Newtonsoft.Json;
using Salient.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiversityPhone.Services {
    public class TransferProgress {
        public string LocalFolderPath { get; private set; }
        public string RemoteFolderPath { get; private set; }
        public IObservable<int> Progress { get; private set; }

        public TransferProgress(string local, string remote, IObservable<int> p) {
            LocalFolderPath = local;
            RemoteFolderPath = remote;
            Progress = p;
        }
    }

    public interface ICloudStorageService {
        IObservable<bool> IsConnectedObservable();
        Task<IEnumerable<string>> GetRemoteFolders();
        Task UploadFolderAsync(string LocalFolderPath, IProgress<int> PercentageProgress);
        Task DownloadFolderAsync(string FolderName, string LocalTargetFolder, IProgress<int> PercentageProgress);
    }



    public class CloudStorageService : ICloudStorageService {
        private const string ZIP_TEMP_FILE = "FolderUpload.zip";
        private const string SKYDRIVE_ROOT_FOLDER = "me/skydrive";
        private const string DIVERSITY_FOLDER_NAME = "DiversityMobile";
        private const string TEMP_FOLDER = "Unzip";

        private readonly Regex ZIP_FILE_PATTERN = new Regex(@"(?<name>[\w\s-_]+).zip$", RegexOptions.IgnoreCase);

        private LiveConnectSession _Session;
        private LiveConnectClient _Client;
        public LiveConnectSession Session {
            get { return _Session; }
            set {
                _Session = value;
                if (value != null) {
                    _Client = new LiveConnectClient(value);
                }

                _IsConnected.OnNext(value != null);
            }
        }

        public void Disconnect() {
            var cl = new LiveAuthClient("00000000480F4F1E");

            cl.Logout();
        }

        private ISubject<bool> _IsConnected = new BehaviorSubject<bool>(false);

        public IObservable<bool> IsConnectedObservable() { return _IsConnected.DistinctUntilChanged(); }

        public class LiveFileOrFolder {
            public const string TYPE_FILE = "file";
            public const string TYPE_FOLDER = "folder";

            public string name { get; set; }
            public string upload_location { get; set; }
            public string id { get; set; }
            public string type { get; set; }
        }

        public class LiveFileResult {
            public IList<LiveFileOrFolder> data { get; set; }
        }

        private async Task<IEnumerable<LiveFileOrFolder>> GetSubFolders(LiveConnectClient client, string SkyDrivePath) {
            var queryUrl = string.Format("{0}/files?filter=folders", SkyDrivePath);

            return await GetFilesOrFolders(client, queryUrl);
        }

        private async Task<IEnumerable<LiveFileOrFolder>> GetFilesOrFolders(LiveConnectClient client, string queryUrl) {
            var queryResult = await client.Get(queryUrl);

            var folders = JsonConvert.DeserializeObject<LiveFileResult>(queryResult.RawResult);

            return folders.data;
        }

        private async Task<IEnumerable<LiveFileOrFolder>> GetFilesInDirectory(LiveConnectClient client, string SkyDrivePath) {
            var queryUrl = string.Format("{0}/files", SkyDrivePath);

            return await GetFilesOrFolders(client, queryUrl);
        }

        private async Task<IEnumerable<LiveFileOrFolder>> GetZipFilesInDirectory(LiveConnectClient client, string SkyDrivePath) {
            var queryUrl = string.Format("{0}/files", SkyDrivePath);

            var filesFolders = await GetFilesOrFolders(client, queryUrl);

            return (from f in filesFolders
                    where f.type == LiveFileOrFolder.TYPE_FILE && ZIP_FILE_PATTERN.IsMatch(f.name)
                    select f).ToList();
        }

        private async Task<LiveFileOrFolder> CreateSubfolder(string SkyDrivePath, string FolderName) {
            var folderObject = new LiveFileOrFolder()
            {
                name = FolderName
            };
            var folderData = JsonConvert.SerializeObject(folderObject);

            var result = await _Client.Post(SkyDrivePath, folderData);

            var newFolder = JsonConvert.DeserializeObject<LiveFileOrFolder>(result.RawResult);

            return newFolder;
        }

        private async Task<LiveFileOrFolder> GetOrCreateDiversityMobileFolder(LiveConnectClient client) {
            var foldersInRoot = await GetSubFolders(client, SKYDRIVE_ROOT_FOLDER);
            var diversityFolder = (from f in foldersInRoot
                                   where f.name == DIVERSITY_FOLDER_NAME
                                   select f).FirstOrDefault();

            if (diversityFolder != null) {
                return diversityFolder;
            }

            diversityFolder = await CreateSubfolder(SKYDRIVE_ROOT_FOLDER, DIVERSITY_FOLDER_NAME);

            return diversityFolder;
        }

        private static IsolatedFastZip GetZipper(IProgress<int> Progress) {
            var events = new FastZipEvents();
            events.ProgressInterval = TimeSpan.FromSeconds(1);
            events.Progress = (s, args) => {
                Progress.Report((int)args.PercentComplete);
            };
            IsolatedFastZip zipper = new IsolatedFastZip(events)
            {
                CreateEmptyDirectories = true,
                RestoreAttributesOnExtract = true,
                RestoreDateTimeOnExtract = true
            };
            return zipper;
        }

        private Task CompressDirectoryAsync(string LocalFolderPath, string zipFile, IProgress<int> PercentageProgress) {
            return Task.Factory.StartNew(() => {
                var zipper = GetZipper(PercentageProgress);
                zipper.CreateZip(zipFile, LocalFolderPath, true, string.Empty, string.Empty);
            });
        }

        private Task DeCompressDirectoryAsync(Stream zipFile, string LocalFolderPath, IProgress<int> PercentageProgress) {
            return Task.Factory.StartNew(() => {
                var zipper = GetZipper(PercentageProgress);
                zipper.ExtractZip(zipFile, LocalFolderPath, IsolatedFastZip.Overwrite.Always, null, string.Empty, string.Empty, true, true);
            });
        }

        public async Task<IEnumerable<string>> GetRemoteFolders() {
            try {
                var client = _Client;

                if (client != null) {
                    var diversityFolder = await GetOrCreateDiversityMobileFolder(client);

                    if (diversityFolder != null) {
                        var zipFiles = await GetZipFilesInDirectory(client, diversityFolder.id);

                        return from f in zipFiles
                               let match = ZIP_FILE_PATTERN.Match(f.name)
                               let name = match.Groups["name"].Value
                               select name;
                    }
                }
            }
            catch (Exception) { }
            return Enumerable.Empty<string>();

        }

        public async Task UploadFolderAsync(string LocalFolderPath, IProgress<int> PercentageProgress) {
            using (var Iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                Contract.Requires(Iso.DirectoryExists(LocalFolderPath));

                var client = _Client;
                if (client == null) {
                    throw new InvalidOperationException("CloudService not connected");
                }

                var firstHalfProgress = new Progress<int>(p => PercentageProgress.Report(p / 2));

                var zipT = CompressDirectoryAsync(LocalFolderPath, ZIP_TEMP_FILE, firstHalfProgress);
                var folderT = GetOrCreateDiversityMobileFolder(client);
                var zipFileName = string.Format("{0}.zip", Path.GetFileName(LocalFolderPath));

                await zipT;
                var diversityFolder = await folderT;

                using (var zipFile = Iso.OpenFile(ZIP_TEMP_FILE, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var secondHalfProgress = new Progress<LiveOperationProgress>(p => PercentageProgress.Report(50 + (int)(p.ProgressPercentage / 2.0)));
                    await client.Upload(diversityFolder.id, zipFileName, zipFile, OverwriteOption.Rename, secondHalfProgress);
                }
            }
        }

        public async Task DownloadFolderAsync(string FolderName, string LocalTargetFolder, IProgress<int> PercentageProgress) {
            using (var Iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                Contract.Requires(Iso.DirectoryExists(LocalTargetFolder));
                var finalFolderName = Path.Combine(LocalTargetFolder, FolderName);
                Contract.Requires(!Iso.DirectoryExists(finalFolderName));

                var client = _Client;
                if (client == null) {
                    throw new InvalidOperationException("CloudService not connected");
                }

                var diversityFolder = await GetOrCreateDiversityMobileFolder(client);
                var zipFiles = await GetZipFilesInDirectory(client, diversityFolder.id);
                var requestedFile = (from zip in zipFiles
                                     where zip.name.StartsWith(FolderName)
                                     select zip).FirstOrDefault();

                if (requestedFile == null) {
                    throw new ArgumentException("FolderName invalid");
                }

                // Clear Temporary unzip Folder
                await Iso.DeleteDirectoryRecursiveAsync(TEMP_FOLDER);
                Iso.CreateDirectory(TEMP_FOLDER);

                // Download Zip File
                var firstHalfProgress = new Progress<LiveOperationProgress>(p => PercentageProgress.Report((int)(p.ProgressPercentage / 2.0)));
                var fileDataUrl = string.Format("{0}/content", requestedFile.id);
                var downloadedStream = await client.Download(fileDataUrl, firstHalfProgress);

                // Decompress into Temp Folder
                var secondHalfProgress = new Progress<int>(p => PercentageProgress.Report(50 + (p / 2)));
                await DeCompressDirectoryAsync(downloadedStream, TEMP_FOLDER, secondHalfProgress);

                // Finally... Move
                Iso.MoveDirectory(TEMP_FOLDER, finalFolderName);
            }
        }
    }
}
