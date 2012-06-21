using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Funq;
using DiversityPhone.Services;
using ReactiveUI;
using DiversityPhone.DiversityService;
using DiversityPhone.ViewModels.Utility;
using DiversityPhone.ViewModels.Maps;

namespace DiversityPhone.ViewModels
{
    public partial class ViewModelLocator
    {
        private const string OFFLINE_STORAGE = "OfflineStorage";
        private static Container _ioc;    

        private static HomeVM _homeVM;        

        public ViewModelLocator()
        {

            if (App.IOC == null) return;
            _ioc = App.IOC;

            #region ViewModel Factories
            _ioc.Register<HomeVM>(container => new HomeVM(                
                container.Resolve<IMessageBus>(),
                container.Resolve<IFieldDataService>(),
                container.Resolve<IDiversityServiceClient>(),
                container.Resolve<ISettingsService>()
                ));
            
            _ioc.Register<EditESVM>(c => new EditESVM(c.Resolve<ISettingsService>()));

            _ioc.Register<ViewESVM>(container => new ViewESVM());
            _ioc.Register<EditEVVM>(c => new EditEVVM(c));


            _ioc.Register<ViewEVVM>(c => new ViewEVVM());

            _ioc.Register<ViewCSVM>(c => new ViewCSVM(c));
            _ioc.Register<EditCSVM>(c => new EditCSVM());
            _ioc.Register<ViewMapPickerVM>(c => new ViewMapPickerVM(c.Resolve<IMapStorageService>()));
            _ioc.Register<ViewDLMVM>(c => new ViewDLMVM(c.Resolve<IMapTransferService>()));
            _ioc.Register<ViewDownloadMapsVM>(c => new ViewDownloadMapsVM(c.Resolve<IMapStorageService>()));
            _ioc.Register<ViewMapVM>(c => new ViewMapVM(c.Resolve<IMapStorageService>(),c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            _ioc.Register<ViewMapESVM>(c => new ViewMapESVM(c.Resolve<IMapStorageService>(), c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            _ioc.Register<ViewMapEVVM>(c => new ViewMapEVVM(c.Resolve<IMapStorageService>(), c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            _ioc.Register<ViewMapIUVM>(c => new ViewMapIUVM(c.Resolve<IMapStorageService>(), c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            _ioc.Register<EditIUVM>(c => new EditIUVM(c));
            _ioc.Register<ViewIUVM>(c => new ViewIUVM(c));
            _ioc.Register<EditAnalysisVM>(c => new EditAnalysisVM(c));
            _ioc.Register<EditMapVM>(c => new EditMapVM(c.Resolve<IMessageBus>()));
            _ioc.Register<SelectNewMMOVM>(c => new SelectNewMMOVM());
            _ioc.Register<NewImageVM>(c => new NewImageVM());
            _ioc.Register<NewAudioVM>(c => new NewAudioVM());
            _ioc.Register<NewVideoVM>(c => new NewVideoVM());
            _ioc.Register<ViewImageVM>(c => new ViewImageVM());
            _ioc.Register<ViewAudioVM>(c => new ViewAudioVM());
            _ioc.Register<ViewVideoVM>(c => new ViewVideoVM());
            _ioc.Register<EditPropertyVM>(c => new EditPropertyVM(c));            

            _ioc.Register<TaxonManagementVM>(c => new TaxonManagementVM(c));

            _ioc.Register<SettingsVM>(c => new SettingsVM(c));

            _ioc.Register<SyncVM>(c => new SyncVM(c));

            #endregion

            #region ViewModel Instantiation
            _homeVM = _ioc.Resolve<HomeVM>();                        
            #endregion

        }       

        public HomeVM Home { get { return _homeVM; } }

        public EditESVM EditES { get { return _ioc.Resolve<EditESVM>(); } }
        public ViewESVM ViewES { get { return _ioc.Resolve<ViewESVM>(); } }

        public EditEVVM EditEV { get { return _ioc.Resolve<EditEVVM>(); } }
        public ViewEVVM ViewEV { get { return _ioc.Resolve<ViewEVVM>(); } }

        public EditCSVM EditCS { get { return _ioc.Resolve<EditCSVM>(); } }
        public ViewCSVM ViewCS { get { return _ioc.Resolve<ViewCSVM>(); } }

        public EditIUVM EditIU { get { return _ioc.Resolve<EditIUVM>(); } }
        public ViewIUVM ViewIU { get { return _ioc.Resolve<ViewIUVM>(); } }
        public EditAnalysisVM EditAnalysis { get { return _ioc.Resolve<EditAnalysisVM>(); } }
        public EditPropertyVM EditProperty { get { return _ioc.Resolve<EditPropertyVM>(); } }


        public ViewMapPickerVM ViewLM { get { return _ioc.Resolve<ViewMapPickerVM>(); } }
        public ViewDLMVM ViewDLM{ get { return _ioc.Resolve<ViewDLMVM>(); } }
        public ViewDownloadMapsVM ViewDownloadMaps { get { return _ioc.Resolve<ViewDownloadMapsVM>(); } }
        public EditMapVM EditMap { get { return _ioc.Resolve<EditMapVM>(); } }
        public ViewMapVM ViewMap { get { return _ioc.Resolve<ViewMapVM>(); } }
        public ViewMapESVM ViewMapES { get { return _ioc.Resolve<ViewMapESVM>(); } }
        public ViewMapEVVM ViewMapEV { get { return _ioc.Resolve<ViewMapEVVM>(); } }
        public ViewMapIUVM ViewMapIU { get { return _ioc.Resolve<ViewMapIUVM>(); } }

        public SelectNewMMOVM SelectNewMMO { get { return _ioc.Resolve <SelectNewMMOVM>(); } }
        public NewImageVM NewImage { get { return _ioc.Resolve<NewImageVM>(); } }
        public NewAudioVM NewAudio { get { return _ioc.Resolve<NewAudioVM>(); } }
        public NewVideoVM NewVideo { get { return _ioc.Resolve<NewVideoVM>(); } }
        public ViewImageVM ViewImage { get { return _ioc.Resolve<ViewImageVM>(); } }
        public ViewAudioVM ViewAudio { get { return _ioc.Resolve<ViewAudioVM>(); } }
        public ViewVideoVM ViewVideo { get { return _ioc.Resolve<ViewVideoVM>(); } }

        public TaxonManagementVM TaxonManagement { get { return _ioc.Resolve<TaxonManagementVM>(); } }
        public SettingsVM Settings { get { return _ioc.Resolve<SettingsVM>(); } }
        public SyncVM Sync { get { return _ioc.Resolve<SyncVM>(); } }

       
    }
}
