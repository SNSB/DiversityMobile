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

namespace DiversityPhone.ViewModels
{
    public partial class ViewModelLocator
    {
        private const string OFFLINE_STORAGE = "OfflineStorage";
        private static Container _ioc;    

        private static HomeVM _homeVM;        

        public ViewModelLocator()
        {

            if (_ioc == null) return;

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
            _ioc.Register<ViewMapVM>(c => new ViewMapVM(c.Resolve<IMapStorageService>(),c.Resolve<IGeoLocationService>()));
            _ioc.Register<EditIUVM>(c => new EditIUVM(c));
            _ioc.Register<ViewIUVM>(c => new ViewIUVM(c));
            _ioc.Register<EditAnalysisVM>(c => new EditAnalysisVM(c));
            _ioc.Register<EditMapVM>(c => new EditMapVM(c.Resolve<IMessageBus>()));
            _ioc.Register<EditMultimediaObjectVM>(c => new EditMultimediaObjectVM());
            _ioc.Register<ViewImageVM>(c => new ViewImageVM());
            _ioc.Register<ViewAudioVM>(c => new ViewAudioVM());
            _ioc.Register<ViewVideoVM>(c => new ViewVideoVM());
            _ioc.Register<EditPropertyVM>(c => new EditPropertyVM(c.Resolve<IVocabularyService>()));            

            _ioc.Register<TaxonManagementVM>(c => new TaxonManagementVM(c.Resolve<IMessageBus>(), 
                                                                        c.Resolve<ITaxonService>(),
                                                                        c.Resolve<IDiversityServiceClient>()));

            _ioc.Register<SettingsVM>(c => new SettingsVM(c));

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
        public ViewMapPickerVM ViewLM { get { return _ioc.Resolve<ViewMapPickerVM>(); } }
        public ViewDLMVM ViewDLM{ get { return _ioc.Resolve<ViewDLMVM>(); } }
        public ViewDownloadMapsVM ViewDownloadMaps { get { return _ioc.Resolve<ViewDownloadMapsVM>(); } }
 
        public EditAnalysisVM EditAnalysis{get{ return _ioc.Resolve<EditAnalysisVM>();}}

        public EditMapVM EditMap { get { return _ioc.Resolve<EditMapVM>(); } }
        public ViewMapVM ViewMap { get { return _ioc.Resolve<ViewMapVM>(); } }
        public EditMultimediaObjectVM EditMMO { get { return _ioc.Resolve<EditMultimediaObjectVM>(); } }
        public EditMultimediaObjectVM ViewMMO { get { return _ioc.Resolve<EditMultimediaObjectVM>(); } }
        public ViewImageVM ViewImage { get { return _ioc.Resolve<ViewImageVM>(); } }
        public ViewAudioVM ViewAudio { get { return _ioc.Resolve<ViewAudioVM>(); } }
        public ViewVideoVM ViewVideo { get { return _ioc.Resolve<ViewVideoVM>(); } }
        public EditPropertyVM EditProperty { get { return _ioc.Resolve<EditPropertyVM>(); } }

        public TaxonManagementVM TaxonManagement { get { return _ioc.Resolve<TaxonManagementVM>(); } }
        public SettingsVM Settings { get { return _ioc.Resolve<SettingsVM>(); } }
       
    }
}
