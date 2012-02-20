using Funq;
using DiversityPhone.Services;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace DiversityPhone.ViewModels
{
    public partial class ViewModelLocator
    {
        static ViewModelLocator()
        {
            _ioc = new Container();
            _ioc.DefaultReuse = ReuseScope.None;

            #region Service Registration
            _ioc.Register<IMessageBus>(RxApp.MessageBus);           

            _ioc.Register<DialogService>(new DialogService(_ioc.Resolve<IMessageBus>()));

            _ioc.Register<IOfflineStorage>(App.OfflineDB);

            _ioc.Register<ISettingsService>(App.Settings);

            _ioc.Register<IDiversityServiceClient>(new DiversityServiceClient(_ioc.Resolve<ISettingsService>())); 

            
            #endregion            
        }       
    }
}
