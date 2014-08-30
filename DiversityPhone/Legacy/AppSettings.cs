/**
 * The AppSettings Class was removed in favor of the Profile Service in Version 0.9.9
 * This File is left in place because some Versions (like 0.9.6) depend on the AppSettings Class,
 * located in the DiversityPhone Assembly to perform data Migration upon being updated to 0.9.9.1 or greater
 **/

namespace DiversityPhone.Model
{
    public static class AppSettingsMixin
    {
#pragma warning disable 0436

        // Should use the AppSettings from the local assembly
        public static Settings ToSettings(this AppSettings apps)
#pragma warning restore 0436
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