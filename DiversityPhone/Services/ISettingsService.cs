using System;
namespace DiversityPhone.Services
{
    public interface ISettingsService
    {
        DiversityPhone.Model.AppSettings getSettings();
        void saveSettings(DiversityPhone.Model.AppSettings settings);
    }
}
