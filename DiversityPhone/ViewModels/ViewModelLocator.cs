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
using Wintellect.Sterling;
using DiversityPhone.Services;
using DiversityPhone.ViewModels;
using ReactiveUI;

namespace DiversityPhone
{
    public class ViewModelLocator
    {
        private const string OFFLINE_STORAGE = "OfflineStorage";
        private static Container _ioc = new Container();
        private static IMessageBus _messenger = MessageBus.Current;

        private static HomeViewModel _homeVM;
        private static EventSeriesViewModel _hierarchyVM;

        public ViewModelLocator()
        {
            #region Service/Factory Registration
            
            _ioc.Register<IOfflineStorage>(App.OfflineDB);
            
            _ioc.Register<INavigationService>(App.Navigation);

            _ioc.Register<HomeViewModel>(container => new HomeViewModel(container.Resolve<INavigationService>()));
            _ioc.Register<EventSeriesViewModel>(container => new EventSeriesViewModel(
                container.Resolve<IOfflineStorage>(),
                container.Resolve<INavigationService>())
                );
            #endregion

            #region ViewModel Instantiation
            _homeVM = _ioc.Resolve<HomeViewModel>();
            _messenger.RegisterViewModel<HomeViewModel>(_homeVM);

            _hierarchyVM = _ioc.Resolve<EventSeriesViewModel>();
            _messenger.RegisterViewModel<EventSeriesViewModel>(_hierarchyVM);
            #endregion

        }

        public HomeViewModel Home { get { return _homeVM; } }
        public EventSeriesViewModel Hierarchy { get { return _hierarchyVM; } }
           
    }
}
