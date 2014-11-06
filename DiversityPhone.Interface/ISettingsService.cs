using DiversityPhone.Model;
using System;

namespace DiversityPhone.Interface
{
    public interface ISettingsService
    {
        IObservable<Settings> SettingsObservable();

        IObservable<Settings> CurrentSettings();

        void SaveSettings(Settings settings);

        void ClearSettings();

        Settings LoadSettingsFromFile(string FilePath);
    }
}