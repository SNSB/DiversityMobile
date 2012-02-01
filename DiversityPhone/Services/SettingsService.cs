using System;
using System.IO.IsolatedStorage;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;

namespace DiversityPhone.Services
{
    public class SettingsService
    {
        private const string SETTINGS_KEY = "Settings";   
     
        AppSettings _settings;
        public SettingsService (IMessageBus msg = null)
	    {
            if (msg != null)
                msg.Listen<AppSettings>(MessageContracts.SAVE)
                    .Subscribe(s => saveSettings(s));

            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(SETTINGS_KEY, out _settings))
                _settings = new AppSettings();
	    }

        public AppSettings getSettings()
        {
            return _settings.Clone();
        }

        public void saveSettings(AppSettings settings)
        {
            _settings = settings;
            IsolatedStorageSettings.ApplicationSettings[SETTINGS_KEY] = settings;
        }

        
    }
}
