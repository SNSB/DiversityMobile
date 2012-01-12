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
using DiversityPhone.Service;

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
                container.Resolve<IOfflineStorage>(),
                container.Resolve<IDiversityService>()
                ));
            
            _ioc.Register<EditESVM>(c => new EditESVM(c.Resolve<IMessageBus>(),c.Resolve<IOfflineStorage>()));

            _ioc.Register<ViewESVM>(container => new ViewESVM(
                container.Resolve<IMessageBus>(),
                container.Resolve<IOfflineStorage>()
                ));
            _ioc.Register<EditEVVM>(c => new EditEVVM(c.Resolve<IMessageBus>(), c.Resolve<IOfflineStorage>()));


            _ioc.Register<ViewEVVM>(c => new ViewEVVM(
                c.Resolve<IMessageBus>(),
                c.Resolve<IOfflineStorage>()
                ));

            _ioc.Register<ViewCSVM>(c => new ViewCSVM(
                c.Resolve<IMessageBus>(),
                c.Resolve<IOfflineStorage>()
                ));
            _ioc.Register<ViewLMVM>(c => new ViewLMVM(c.Resolve<IMessageBus>()));
            _ioc.Register<EditIUVM>(c => new EditIUVM(c.Resolve<IMessageBus>(), c.Resolve<IOfflineStorage>()));
            _ioc.Register<ViewIUVM>(c => new ViewIUVM(c.Resolve<IMessageBus>(), c.Resolve<IOfflineStorage>() ));

            _ioc.Register<EditMapVM>(c => new EditMapVM(c.Resolve<IMessageBus>()));
            _ioc.Register<EditMultimediaObjectVM>(c => new EditMultimediaObjectVM(c.Resolve<IMessageBus>()));
            _ioc.Register<EditPropertyVM>(c => new EditPropertyVM(c.Resolve<IMessageBus>(),c.Resolve<IOfflineStorage>()));
            _ioc.Register<EditUserProfileVM>(c => new EditUserProfileVM(c.Resolve<IMessageBus>()));

            #endregion

            #region ViewModel Instantiation
            _homeVM = _ioc.Resolve<HomeVM>();                        
            #endregion

        }
        public EditUserProfileVM EditProfile { get { return _ioc.Resolve<EditUserProfileVM>(); } }

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
        public EditMultimediaObjectVM EditEVMMO { get { return _ioc.Resolve<EditMultimediaObjectVM>(); } }
        public EditPropertyVM EditProperty { get { return _ioc.Resolve<EditPropertyVM>(); } }
       
    }
}
