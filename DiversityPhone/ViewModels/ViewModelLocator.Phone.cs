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

            _ioc.Register<IFieldDataService>(App.OfflineDB);
            _ioc.Register<ITaxonService>(new TaxonService());
            _ioc.Register<IVocabularyService>(new VocabularyService(_ioc.Resolve<IMessageBus>()));
            _ioc.Register<IMapStorageService>(new MapStorageService(_ioc.Resolve<IMessageBus>()));
            _ioc.Register<IMapTransferService>(new MapTranferService(_ioc.Resolve<IMessageBus>()));
            _ioc.Register<ISettingsService>(App.Settings);

            _ioc.Register<IDiversityServiceClient>(new DiversityServiceObservableClient(_ioc.Resolve<ISettingsService>()));
            _ioc.Register<IGeoLocationService>(App.GeoLocation as GeoLocationService);

            
            #endregion            
        }       
    }
}
