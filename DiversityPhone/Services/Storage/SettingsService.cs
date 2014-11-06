using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace DiversityPhone.Services
{
    public class SettingsService : ISettingsService, ICredentialsService
    {
        public const string SETTINGS_FILE = "AppSettings.xml";

        private readonly IScheduler Dispatcher;
        private readonly ICurrentProfile Profile;
        private readonly XmlSerializer SettingsSerializer;

        private ISubject<Unit> _ReloadSettings = new Subject<Unit>();

        private ISubject<Settings> _SettingsIn;
        private ISubject<Settings> _SettingsOut = new Subject<Settings>();
        private IObservable<Settings> _SettingsDispatcher;

        private BehaviorSubject<Settings> _SettingsMostRecent = new BehaviorSubject<Settings>(null);

        private volatile int _OpsInFlight = 0;

        public SettingsService(
            ICurrentProfile Profile,
            [Dispatcher] IScheduler Dispatcher,
            [ThreadPool] IScheduler ThreadPool
            )
        {
            Contract.Requires(Profile != null);
            Contract.Requires(Dispatcher != null);
            Contract.Requires(ThreadPool != null);

            this.Dispatcher = Dispatcher;
            this.Profile = Profile;
            this.SettingsSerializer = new XmlSerializer(typeof(Settings));

            _SettingsDispatcher =
                _SettingsOut
                .ObserveOn(Dispatcher);

            _SettingsOut
                .Subscribe(_SettingsMostRecent);

            _SettingsIn = new Subject<Settings>();

            _SettingsIn
                .Do(EnterOperation)
                .ObserveOn(ThreadPool)
                .Select(PersistSettings)
                .Do(ExitOperation)
                .Subscribe(_SettingsOut);

            Profile
                .CurrentProfilePathObservable()
                .Merge(_ReloadSettings.Select(_ => Profile.CurrentProfilePath()))
                .Do(EnterOperation)
                .Select(ProfileToSettingsPath)
                .ObserveOn(ThreadPool)
                .Select(LoadSettingsFromFile)
                .Do(ExitOperation)
                .Subscribe(_SettingsOut);
        }

        public void ReloadSettings()
        {
            _ReloadSettings.OnNext(Unit.Default);
        }

        public Settings LoadSettingsFromFile(string FilePath)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (iso.FileExists(FilePath))
                    {
                        using (var settingsFile = iso.OpenFile(FilePath, FileMode.Open, FileAccess.Read))
                        {
                            Settings settings;
                            var settingsXml = XmlReader.Create(settingsFile);
                            if (SettingsSerializer.CanDeserialize(settingsXml))
                            {
                                settings = (Settings)SettingsSerializer.Deserialize(settingsXml);
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

        private Settings PersistSettings(Settings s)
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

        private void EnterOperation(object _)
        {
            lock (this)
            {
                _OpsInFlight++;
            }
        }

        private void ExitOperation(object _)
        {
            lock (this)
            {
                if (_OpsInFlight <= 0)
                {
                    throw new InvalidOperationException("ExitLoad called more often than EnterLoad");
                }

                _OpsInFlight--;
            }
        }

        public string GetSettingsPath()
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

        public void SaveSettings(Settings settings)
        {
            _SettingsIn.OnNext(settings);
        }

        public IObservable<UserCredentials> CurrentCredentials()
        {
            return CurrentSettings()
                .Select(s => s.ToCreds());
        }

        public IObservable<Settings> SettingsObservable()
        {
            return _SettingsDispatcher;
        }

        public IObservable<Settings> CurrentSettings()
        {
            // Block until all Loads have finished, then return most recent Settings
            return _SettingsMostRecent
                .SkipWhile(_ => _OpsInFlight != 0)
                .FirstAsync()
                .ObserveOn(Dispatcher);
        }
    }
}