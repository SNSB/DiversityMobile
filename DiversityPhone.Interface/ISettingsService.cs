using DiversityPhone.Model;
using System;

namespace DiversityPhone.Interface
{
    public interface ISettingsService
    {
        IObservable<AppSettings> SettingsObservable();
        AppSettings CurrentSettings { get; }
        void SaveSettings(AppSettings settings);
        void ClearSettings();
        AppSettings LoadSettingsFromFile(string FilePath);
    }
}
