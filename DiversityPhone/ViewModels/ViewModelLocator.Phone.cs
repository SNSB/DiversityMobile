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
        static Container ServiceContainer()
        {
            var container = new Container();

            #region Service Registration
            container.Register<IMessageBus>(RxApp.MessageBus);

            container.Register<INavigationService>(new NavigationService(container.Resolve<IMessageBus>()));

            container.Register<DialogService>(new DialogService(container.Resolve<IMessageBus>()));

            container.Register<IOfflineStorage>(App.OfflineDB);            

            container.Register<IDiversityService>(App.Repository);

            
            #endregion


            return container;
        }

            

            
        public ViewModelLocator()
            : this(ServiceContainer())
        {
            
        }
    }
}
