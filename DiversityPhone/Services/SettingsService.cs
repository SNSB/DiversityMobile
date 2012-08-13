using System;
using System.IO.IsolatedStorage;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using System.Reactive.Linq;

namespace DiversityPhone.Services
{
    public class SettingsService : ISettingsService
    {
        private const string SETTINGS_KEY = "Settings";

        IMessageBus Messenger;
        AppSettings _settings;
        public SettingsService (IMessageBus msg)
	    {
            Messenger = msg;
            msg.Listen<AppSettings>(MessageContracts.SAVE)
                .Subscribe(s => saveSettings(s));

            Messenger.RegisterMessageSource(
            Messenger.Listen<EventMessage>(MessageContracts.INIT)
                .Where(_ => _settings != null)
                .Select(_ => _settings.ToCreds())
                );
            
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(SETTINGS_KEY, out _settings))
                _settings = null;
	    }

        public AppSettings getSettings()
        {
            return (_settings != null) ? _settings.Clone() : null;
        }

        public void saveSettings(AppSettings settings)
        {
            _settings = settings;
            if (settings != null)
                Messenger.SendMessage(settings.ToCreds());
            IsolatedStorageSettings.ApplicationSettings[SETTINGS_KEY] = settings;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        
    }
}
