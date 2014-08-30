/**
 * The AppSettings Class was removed in favor of the Profile Service in Version 0.9.9
 * This File is left in place because some Versions (before like 0.9.8) depend on the AppSettings Class,
 * located in the DiversityPhone.Model Assembly to perform data Migration upon being updated to 0.9.9.1 or greater
 **/

namespace DiversityPhone.Model
{
    public static class AppSettingsMixin
    {
        public static Settings ToSettings(this AppSettings apps)
        {
            return new Settings()
            {
                UserName = apps.UserName,
                Password = apps.Password,
                AgentName = apps.AgentName,
                AgentURI = apps.AgentURI,
                UseGPS = apps.UseGPS,

                SaveMultimediaExternally = apps.SaveMultimediaExternally,

                HomeDBName = apps.HomeDBName,

                CurrentProject = apps.CurrentProject,
                CurrentProjectName = apps.CurrentProjectName,
                CurrentSeriesID = apps.CurrentSeriesID
            };
        }
    }

    public class AppSettings
    {
        public AppSettings()
        {
            UseGPS = true;
            SaveMultimediaExternally = true;
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string AgentName { get; set; }

        public string AgentURI { get; set; }

        public bool UseGPS { get; set; }

        public bool SaveMultimediaExternally { get; set; }

        public string HomeDBName { get; set; }

        public int CurrentProject { get; set; }

        public string CurrentProjectName { get; set; }

        public int? CurrentSeriesID { get; set; }
    }
}