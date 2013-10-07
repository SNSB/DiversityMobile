using DiversityPhone.Interface;
using ReactiveUI;

namespace DiversityPhone.Services
{
    public interface ICleanupData
    {
        void ClearLocalData();
    }

    public class CleanupService : ICleanupData
    {
        public void ClearLocalData()
        {
            Taxa.ClearTaxonLists();
            FieldData.ClearDatabase();
            Multimedia.ClearAllMultimedia();
            Maps.ClearMaps();
            Settings.ClearSettings();
        }

        private IFieldDataService FieldData;
        private IMapStorageService Maps;
        private IStoreMultimedia Multimedia;
        private ISettingsService Settings;
        private ITaxonService Taxa;
        private IMessageBus Messenger;

        public CleanupService(
            IFieldDataService FieldData,
            IKeyMappingService Mapping,
            IMapStorageService Maps,
            ITaxonService Taxa,
            IStoreMultimedia Multimedia,
            ISettingsService Settings,
            IMessageBus Messenger
            )
        {
            this.FieldData = FieldData;
            this.Maps = Maps;
            this.Taxa = Taxa;
            this.Multimedia = Multimedia;
            this.Settings = Settings;
            this.Messenger = Messenger;
        }
    }
}
