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
        private static ListESVM _hierarchyVM;
        private static EditESVM _editESVM;
        private static SetupVM _setupVM;

        public ViewModelLocator(Container services)
        {
            _ioc = services;
            #region ViewModel Factories
            _ioc.Register<HomeVM>(container => new HomeVM(
                container.Resolve<INavigationService>(),
                container.Resolve<IMessageBus>()
                ));
            _ioc.Register<ListESVM>(container => new ListESVM(                
                container.Resolve<INavigationService>(),
                container.Resolve<IOfflineStorage>(),
                container.Resolve<IMessageBus>()
                ));
            _ioc.Register<EditESVM>(c => new EditESVM(c.Resolve<INavigationService>(), c.Resolve<IMessageBus>()));

            _ioc.Register<SetupVM>(c => new SetupVM(c.Resolve<INavigationService>(),c.Resolve<IOfflineStorage>(),c.Resolve<IDiversityService>()));
            #endregion

            #region ViewModel Instantiation
            _homeVM = _ioc.Resolve<HomeVM>();            

            _hierarchyVM = _ioc.Resolve<ListESVM>();            

            _editESVM = _ioc.Resolve<EditESVM>();
            #endregion

        }

        public HomeVM Home { get { return _homeVM; } }
        public ListESVM EventSeries { get { return _hierarchyVM; } }
        public EditESVM EditES { get { return _editESVM; } }
        public SetupVM Setup { get { return _setupVM; } }   
    }
}
