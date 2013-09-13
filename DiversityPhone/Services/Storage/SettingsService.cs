using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.IO.IsolatedStorage;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.Services
{
    

    public class SettingsService : ISettingsService, ICredentialsService
    {
        private const string SETTINGS_KEY = "Settings";

        BehaviorSubject<AppSettings> _SettingSubject = new BehaviorSubject<AppSettings>(null);
        public SettingsService()
	    {
            AppSettings settings;

            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(SETTINGS_KEY, out settings))
                _SettingSubject.OnNext(settings);
	    }

        public void ClearSettings()
        {
            SaveSettings(null);
        }



        public void SaveSettings(AppSettings settings)
        {
            IsolatedStorageSettings.ApplicationSettings[SETTINGS_KEY] = settings;
            IsolatedStorageSettings.ApplicationSettings.Save();
            _SettingSubject.OnNext(settings);
        }



        public UserCredentials CurrentCredentials()
        {
            var settings = _SettingSubject.FirstOrDefault();
            if (settings != null)
                return settings.ToCreds();
            return null;
        }

        public IObservable<AppSettings> SettingsObservable()
        {
            return _SettingSubject.AsObservable();
        }
        public AppSettings CurrentSettings { get { return _SettingSubject.FirstOrDefault(); } }
    }
}
