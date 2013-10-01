using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml;
using System.Xml.Serialization;

namespace DiversityPhone.Services
{
    public class SettingsService : ISettingsService, ICredentialsService
    {
        public const string SETTINGS_FILE = "AppSettings.xml";

        private readonly ICurrentProfile Profile;
        private readonly XmlSerializer SettingsSerializer;

        private ISubject<AppSettings> _SettingsIn;
        private ISubject<AppSettings> _SettingsOut = new Subject<AppSettings>();
        private IObservable<AppSettings> _SettingsReplay;
        public SettingsService(
            ICurrentProfile Profile,
            [Dispatcher] IScheduler Dispatcher,
            [ThreadPool] IScheduler ThreadPool
            )
        {
            Contract.Requires(Profile != null);
            Contract.Requires(Dispatcher != null);
            Contract.Requires(ThreadPool != null);

            this.Profile = Profile;
            this.SettingsSerializer = new XmlSerializer(typeof(AppSettings));

            _SettingsReplay = _SettingsOut
                .ObserveOn(Dispatcher)
                .Replay(1)
                .RefCount();

            _SettingsIn = new Subject<AppSettings>();

            _SettingsIn
                .ObserveOn(ThreadPool)
                .Select(PersistSettings)
                .ObserveOn(Dispatcher)
                .Subscribe(_SettingsOut);

            Profile
                .CurrentProfilePathObservable()
                .Select(ProfileToSettingsPath)
                .ObserveOn(ThreadPool)
                .Select(LoadSettingsFromFile)
                .ObserveOn(Dispatcher)
                .Subscribe(_SettingsOut);
        }

        public AppSettings LoadSettingsFromFile(string FilePath)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (iso.FileExists(FilePath))
                    {
                        using (var settingsFile = iso.OpenFile(FilePath, FileMode.Open, FileAccess.Read))
                        {
                            AppSettings settings;
                            var settingsXml = XmlReader.Create(settingsFile);
                            if (SettingsSerializer.CanDeserialize(settingsXml))
                            {
                                settings = (AppSettings)SettingsSerializer.Deserialize(settingsXml);
                                return settings;
                            }
                        }
                    }
                }
                catch (IsolatedStorageException) { /*TODO Log*/ }
                catch (XmlException) { /*TODO Log*/ }

                return null;
            }
        }

        private AppSettings PersistSettings(AppSettings s)
        {
            var settingsPath = GetSettingsPath();

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (s == null)
                {
                    if (iso.FileExists(settingsPath))
                    {
                        iso.DeleteFile(settingsPath);
                    }
                }
                else
                {
                    using (var settingsFile = iso.OpenFile(settingsPath, FileMode.Create))
                    {
                        SettingsSerializer.Serialize(settingsFile, s);
                    }
                }
            }

            return s;
        }

        private string GetSettingsPath()
        {
            var currentProfile = Profile.CurrentProfilePath();
            return ProfileToSettingsPath(currentProfile);
        }

        private static string ProfileToSettingsPath(string currentProfile)
        {
            return Path.Combine(currentProfile, SETTINGS_FILE);
        }

        public void ClearSettings()
        {
            SaveSettings(null);
        }

        public void SaveSettings(AppSettings settings)
        {
            _SettingsIn.OnNext(settings);
        }



        public IObservable<UserCredentials> CurrentCredentials()
        {
            return _SettingsReplay.Select(s => s.ToCreds());
        }

        public IObservable<AppSettings> SettingsObservable()
        {
            return _SettingsReplay;
        }
        public AppSettings CurrentSettings { get { return _SettingsReplay.FirstOrDefault(); } }
    }
}
