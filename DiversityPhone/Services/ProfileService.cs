using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace DiversityPhone.Services {
    public interface ICurrentProfile {
        string CurrentProfilePath();
        IObservable<string> CurrentProfilePathObservable();
        string ProfilePathForID(int profileID);
        int CurrentProfileID();
        void SetCurrentProfileID(int profileID);
        int CreateProfileID();
        Task ClearUnusedProfiles();
    }

    public class ProfileService : ICurrentProfile {
        public class ProfileData {
            public int CurrentProfileID { get; set; }
            public int NextProfileID { get; set; }

            public ProfileData() {
                CurrentProfileID = 0;
                NextProfileID = 1;
            }
        }

        private const string PROFILE_DIR = "Profile";
        private const string PROFILE_KEY = "ProfileData";
        private ProfileData PROFILE_DATA;
        private readonly IMessageBus Messenger;

        private ISubject<string> _CurrentProfilePath = new Subject<string>();
        private IObservable<string> _CurrentProfilePathReplay;

        public ProfileService(
            IMessageBus Messenger,
            [Dispatcher] IScheduler Dispatcher
            ) {
            this.Messenger = Messenger;

            _CurrentProfilePathReplay = _CurrentProfilePath.Replay(1).PermaRef();

            Initialize();
        }

        private void Initialize() {
            LoadProfileData();

            InitializeCurrentProfileIfNecessary();

            SetupProfileAndSendNotifications(PROFILE_DATA.CurrentProfileID);
        }

        private void LoadProfileData() {
            ProfileData data = null;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(PROFILE_KEY, out data)) {
                PROFILE_DATA = data;
            }
            else {
                PROFILE_DATA = new ProfileData();
                IsolatedStorageSettings.ApplicationSettings[PROFILE_KEY] = PROFILE_DATA;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        public string CurrentProfilePath() {
            return ProfilePathForID(PROFILE_DATA.CurrentProfileID);
        }

        public IObservable<string> CurrentProfilePathObservable() {
            return _CurrentProfilePathReplay;
        }

        public string ProfilePathForID(int profileID) {
            return Path.Combine(PROFILE_DIR, profileID.ToString());
        }

        public int CurrentProfileID() {
            return PROFILE_DATA.CurrentProfileID;
        }

        public void SetCurrentProfileID(int profileID) {
            if (PROFILE_DATA.CurrentProfileID != profileID) {
                SetupProfileAndSendNotifications(profileID);
            }
        }

        private void SetupProfileAndSendNotifications(int profileID) {
            var profilePath = ProfilePathForID(profileID);
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                Contract.Requires(iso.DirectoryExists(profilePath), "Illegal Profile ID");

                PROFILE_DATA.CurrentProfileID = profileID;
            }

            SendCurrentProfilePath();

            SendInitSignal();
        }

        private void SendInitSignal() {
            Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.INIT);
        }

        private void SendCurrentProfilePath() {
            _CurrentProfilePath.OnNext(CurrentProfilePath());
        }

        public int CreateProfileID() {
            var nextProfilePath = ProfilePathForID(PROFILE_DATA.NextProfileID);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                CreateDirIfNecessary(iso, nextProfilePath);
            }

            return PROFILE_DATA.NextProfileID++;
        }

        private void InitializeCurrentProfileIfNecessary() {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                CreateDirIfNecessary(iso, PROFILE_DIR);

                var newProfilePath = CurrentProfilePath();

                CreateDirIfNecessary(iso, newProfilePath);
            }
        }

        private static void CreateDirIfNecessary(IsolatedStorageFile iso, string dir) {
            if (!iso.DirectoryExists(dir)) {
                iso.CreateDirectory(dir);
            }
        }


        public async Task ClearUnusedProfiles() {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                var currentProfile = CurrentProfileID().ToString();
                var profileQuery = string.Format("{0}/", PROFILE_DIR);
                var unusedProfiles = from p in iso.GetDirectoryNames(profileQuery)
                                     where p != currentProfile
                                     select p;

                foreach (var profile in unusedProfiles) {
                    var profilePath = string.Format("{0}/{1}", PROFILE_DIR, profile);
                    await iso.DeleteDirectoryRecursiveAsync(profilePath);
                }
            }
        }
    }
}
