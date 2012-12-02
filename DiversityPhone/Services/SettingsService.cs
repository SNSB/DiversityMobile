using System;
using System.IO.IsolatedStorage;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.Services
{
    public interface ISettingsService
    {
        DiversityPhone.Model.AppSettings getSettings();
        void saveSettings(DiversityPhone.Model.AppSettings settings);
    }

    public class SettingsService : ISettingsService
    {
        private const string SETTINGS_KEY = "Settings";

        IMessageBus Messenger;
        AppSettings _settings;
        ISubject<AppSettings> _SettingSubject = new Subject<AppSettings>();
        public SettingsService (IMessageBus msg)
	    {
            Messenger = msg;
            msg.Listen<AppSettings>(MessageContracts.SAVE)
                .Subscribe(s => saveSettings(s));

            Messenger.RegisterMessageSource(
                _SettingSubject
            );
            Messenger.RegisterMessageSource(
                _SettingSubject
                .Where(settings => settings != null)
                .Select(settings => settings.ToCreds())
                );

            Messenger.Listen<EventMessage>(MessageContracts.INIT)
                .Where(_ => _settings != null)
                .Select(_ => _settings)
                .Subscribe(_SettingSubject);
            
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
            _SettingSubject.OnNext(settings);
            IsolatedStorageSettings.ApplicationSettings[SETTINGS_KEY] = settings;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        
    }
}
