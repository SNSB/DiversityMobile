using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.Services
{
    public interface ICurrentProfile
    {
        string CurrentProfilePath();
        IObservable<string> CurrentProfilePathObservable();
        string ProfilePathForID(int profileID);
        int CurrentProfileID();
        void SetCurrentProfileID(int profileID);
        int CreateProfileID();
    }

    public class ProfileService : ICurrentProfile
    {
        public class ProfileData
        {
            public int CurrentProfileID { get; set; }
            public int NextProfileID { get; set; }

            public ProfileData()
            {
                CurrentProfileID = 0;
                NextProfileID = 1;
            }
        }

        private const string PROFILE_DIR = "Profile";
        private const string PROFILE_KEY = "ProfileData";
        private readonly ProfileData PROFILE_DATA;
        private readonly IMessageBus Messenger;

        private ISubject<string> _CurrentProfilePathSubject = new BehaviorSubject<string>(null);

        public ProfileService(
            IMessageBus Messenger
            )
        {
            this.Messenger = Messenger;

            ProfileData data = null;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(PROFILE_KEY, out data))
            {
                PROFILE_DATA = data;
            }
            else
            {
                PROFILE_DATA = new ProfileData();
                IsolatedStorageSettings.ApplicationSettings[PROFILE_KEY] = PROFILE_DATA;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            InitializeCurrentProfileIfNecessary();

            SetupProfileAndSendNotifications(PROFILE_DATA.CurrentProfileID);
        }

        public string CurrentProfilePath()
        {
            return ProfilePathForID(PROFILE_DATA.CurrentProfileID);
        }

        public IObservable<string> CurrentProfilePathObservable()
        {
            return _CurrentProfilePathSubject
                .AsObservable();
        }

        public string ProfilePathForID(int profileID)
        {
            return Path.Combine(PROFILE_DIR, profileID.ToString());
        }

        public int CurrentProfileID()
        {
            return PROFILE_DATA.CurrentProfileID;
        }

        public void SetCurrentProfileID(int profileID)
        {
            if (PROFILE_DATA.CurrentProfileID != profileID)
            {
                SetupProfileAndSendNotifications(profileID);
            }
        }

        private void SetupProfileAndSendNotifications(int profileID)
        {
            var profilePath = ProfilePathForID(profileID);
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Contract.Requires(iso.DirectoryExists(profilePath), "Illegal Profile ID");

                PROFILE_DATA.CurrentProfileID = profileID;
            }

            SendCurrentProfilePath();

            SendInitSignal();
        }

        private void SendInitSignal()
        {
            Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.INIT);
        }

        private void SendCurrentProfilePath()
        {
            _CurrentProfilePathSubject.OnNext(CurrentProfilePath());
        }

        public int CreateProfileID()
        {
            var nextProfilePath = ProfilePathForID(PROFILE_DATA.NextProfileID);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                CreateDirIfNecessary(iso, nextProfilePath);
            }

            return PROFILE_DATA.NextProfileID++;
        }

        private void InitializeCurrentProfileIfNecessary()
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                CreateDirIfNecessary(iso, PROFILE_DIR);

                var newProfilePath = CurrentProfilePath();

                CreateDirIfNecessary(iso, newProfilePath);
            }
        }

        private static void CreateDirIfNecessary(IsolatedStorageFile iso, string dir)
        {
            if (!iso.DirectoryExists(dir))
            {
                iso.CreateDirectory(dir);
            }
        }
    }
}
