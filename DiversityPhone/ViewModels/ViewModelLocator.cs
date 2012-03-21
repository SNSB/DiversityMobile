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
            
            _ioc.Register<EditESVM>(c => new EditESVM());

            _ioc.Register<ViewESVM>(container => new ViewESVM());
            _ioc.Register<EditEVVM>(c => new EditEVVM());


            _ioc.Register<ViewEVVM>(c => new ViewEVVM());

            _ioc.Register<ViewCSVM>(c => new ViewCSVM());
            _ioc.Register<EditCSVM>(c => new EditCSVM());
            _ioc.Register<ViewLMVM>(c => new ViewLMVM(c.Resolve<IMessageBus>()));
            _ioc.Register<EditIUVM>(c => new EditIUVM(c.Resolve<ITaxonService>(), c.Resolve<IVocabularyService>()));
            _ioc.Register<ViewIUVM>(c => new ViewIUVM());
            _ioc.Register<EditAnalysisVM>(c => new EditAnalysisVM(c.Resolve<IVocabularyService>()));
            _ioc.Register<EditMapVM>(c => new EditMapVM(c.Resolve<IMessageBus>()));
            _ioc.Register<EditMultimediaObjectVM>(c => new EditMultimediaObjectVM());
            _ioc.Register<EditPropertyVM>(c => new EditPropertyVM(c.Resolve<IMessageBus>(),c.Resolve<IFieldDataService>(), c.Resolve<IVocabularyService>()));            

            _ioc.Register<TaxonManagementVM>(c => new TaxonManagementVM(c.Resolve<IMessageBus>(), 
                                                                        c.Resolve<ITaxonService>(),
                                                                        c.Resolve<IDiversityServiceClient>()));

            _ioc.Register<SettingsVM>(c => new SettingsVM(
                c.Resolve<ISettingsService>(),
                c.Resolve<IDiversityServiceClient>(),
                c.Resolve<IFieldDataService>(),
                c.Resolve<IVocabularyService>()
                ));

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
        public ViewLMVM ViewLM { get { return _ioc.Resolve<ViewLMVM>(); } }
     
        public EditAnalysisVM EditAnalysis{get{ return _ioc.Resolve<EditAnalysisVM>();}}

        public EditMapVM EditMap { get { return _ioc.Resolve<EditMapVM>(); } }
        public EditMultimediaObjectVM EditMMO { get { return _ioc.Resolve<EditMultimediaObjectVM>(); } }
        public EditMultimediaObjectVM ViewMMO { get { return _ioc.Resolve<EditMultimediaObjectVM>(); } }
        public EditPropertyVM EditProperty { get { return _ioc.Resolve<EditPropertyVM>(); } }

        public TaxonManagementVM TaxonManagement { get { return _ioc.Resolve<TaxonManagementVM>(); } }
        public SettingsVM Settings { get { return _ioc.Resolve<SettingsVM>(); } }
       
    }
}
