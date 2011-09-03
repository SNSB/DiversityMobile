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
        private static IMessageBus _messenger = MessageBus.Current;

        private static HomeVM _homeVM;        

        public ViewModelLocator(Container services)
        {
            _ioc = services;
            #region ViewModel Factories
            _ioc.Register<HomeVM>(container => new HomeVM(                
                container.Resolve<IMessageBus>(),
                container.Resolve<IOfflineStorage>(),
                container.Resolve<IDiversityService>()
                ));

            _ioc.Register<ListESVM>(container => new ListESVM(                
                container.Resolve<INavigationService>(),
                container.Resolve<IOfflineStorage>(),
                container.Resolve<IMessageBus>()
                ));
            _ioc.Register<EditESVM>(c => new EditESVM(c.Resolve<INavigationService>(), c.Resolve<IMessageBus>()));

            _ioc.Register<ListEVVM>(container => new ListEVVM(
                container.Resolve<IMessageBus>(),
                container.Resolve<IOfflineStorage>()
                ));
            _ioc.Register<EditEVVM>(c => new EditEVVM(c.Resolve<IMessageBus>(), c.Resolve<IOfflineStorage>()));


            _ioc.Register<ListIUVM>(c => new ListIUVM(
                c.Resolve<IMessageBus>(),
                c.Resolve<IOfflineStorage>()
                ));
            _ioc.Register<EditIUVM>(c => new EditIUVM(c.Resolve<IMessageBus>(), c.Resolve<IOfflineStorage>()));

            _ioc.Register<SetupVM>(c => new SetupVM(c.Resolve<INavigationService>(),c.Resolve<IOfflineStorage>(),c.Resolve<IDiversityService>()));

            
            #endregion

            #region ViewModel Instantiation
            _homeVM = _ioc.Resolve<HomeVM>();                        
            #endregion

        }

        public HomeVM Home { get { return _homeVM; } }
        public ListESVM ListES { get { return _ioc.Resolve<ListESVM>(); } }
        public EditESVM EditES { get { return _ioc.Resolve<EditESVM>(); } }
        public ListEVVM ListEV { get { return _ioc.Resolve<ListEVVM>(); } }
        public EditEVVM EditEV { get { return _ioc.Resolve<EditEVVM>(); } }
        public ListIUVM ListIU { get { return _ioc.Resolve<ListIUVM>(); } }
        public EditIUVM EditIU { get { return _ioc.Resolve<EditIUVM>(); } }

        public SetupVM Setup { get { return _ioc.Resolve<SetupVM>(); ; } }
       
    }
}
