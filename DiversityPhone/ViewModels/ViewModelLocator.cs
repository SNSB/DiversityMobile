namespace DiversityPhone.ViewModels
{
    using Ninject;

    public partial class ViewModelLocator
    {
        private const string OFFLINE_STORAGE = "OfflineStorage";
        private static IKernel IOC { get { return App.Kernel; } }

        public ViewModelLocator()
        {
        }

        public UploadVM Upload { get { return IOC.Get<UploadVM>(); } }

        public DownloadVM Download { get { return IOC.Get<DownloadVM>(); } }

        public HomeVM Home { get { return IOC.Get<HomeVM>(); } }

        public EditESVM EditES { get { return IOC.Get<EditESVM>(); } }
        public ViewESVM ViewES { get { return IOC.Get<ViewESVM>(); } }

        public EditEVVM EditEV { get { return IOC.Get<EditEVVM>(); } }
        public ViewEVVM ViewEV { get { return IOC.Get<ViewEVVM>(); } }

        public EditCSVM EditCS { get { return IOC.Get<EditCSVM>(); } }
        public ViewCSVM ViewCS { get { return IOC.Get<ViewCSVM>(); } }

        public EditIUVM EditIU { get { return IOC.Get<EditIUVM>(); } }
        public ViewIUVM ViewIU { get { return IOC.Get<ViewIUVM>(); } }
        public EditAnalysisVM EditAnalysis { get { return IOC.Get<EditAnalysisVM>(); } }
        public EditPropertyVM EditProperty { get { return IOC.Get<EditPropertyVM>(); } }

        public MapManagementVM MapManagement { get { return IOC.Get<MapManagementVM>(); } }
        public ViewMapVM ViewMap { get { return IOC.Get<ViewMapVM>(); } }

        public ViewImageVM ViewImage { get { return IOC.Get<ViewImageVM>(); } }
        public NewImageVM NewImage { get { return IOC.Get<NewImageVM>(); } }
        public AudioVM NewAudio { get { return IOC.Get<AudioVM>(); } }
        public VideoVM NewVideo { get { return IOC.Get<VideoVM>(); } }

        public TaxonManagementVM TaxonManagement { get { return IOC.Get<TaxonManagementVM>(); } }
        public SettingsVM Settings { get { return IOC.Get<SettingsVM>(); } }

        public SetupVM Setup { get { return IOC.Get<SetupVM>(); } }

        public RefreshVocabularyVM Vocabulary { get { return IOC.Get<RefreshVocabularyVM>(); } }

        public ImportExportVM ImportExport { get { return IOC.Get<ImportExportVM>(); } }

    }
}
