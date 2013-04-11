using System;
using System.IO.IsolatedStorage;
using DiversityPhone.Model;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DiversityPhone.Interface;

namespace DiversityPhone.Services
{
    

    public class SettingsService : ISettingsService, ICurrentCredentials
    {
        private const string SETTINGS_KEY = "Settings";

        ISubject<AppSettings> _SettingSubject = new BehaviorSubject<AppSettings>(null);
        public SettingsService()
	    {
            AppSettings settings;

            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(SETTINGS_KEY, out settings))
                _SettingSubject.OnNext(settings);
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

        public IObservable<AppSettings> CurrentSettings()
        {
            return _SettingSubject.AsObservable();
        }
    }
}
