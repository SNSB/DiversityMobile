namespace DiversityPhone.Services {
    using DiversityPhone.Interface;
    using ReactiveUI;

    public interface ICleanupData {
        void ClearLocalData();
    }

    public class CleanupService : ICleanupData {
        public void ClearLocalData() {
            Taxa.ClearTaxonLists();
            FieldData.ClearDatabase();
            Multimedia.ClearAllMultimedia();
            Maps.ClearMaps();
        }

        private IFieldDataService FieldData;
        private IMapStorageService Maps;
        private IStoreMultimedia Multimedia;
        private ITaxonService Taxa;
        private IMessageBus Messenger;

        public CleanupService(
            IFieldDataService FieldData,
            IKeyMappingService Mapping,
            IMapStorageService Maps,
            ITaxonService Taxa,
            IStoreMultimedia Multimedia,
            IMessageBus Messenger
            ) {
            this.FieldData = FieldData;
            this.Maps = Maps;
            this.Taxa = Taxa;
            this.Multimedia = Multimedia;
            this.Messenger = Messenger;
        }
    }
}
