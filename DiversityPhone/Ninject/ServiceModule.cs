namespace DiversityPhone
{
    using DiversityPhone.Interface;
    using DiversityPhone.Services;
    using Ninject;
    using ReactiveUI;
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Concurrency;
using Ninject.Modules;

    class BaseServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IScheduler>().ToConstant(DispatcherScheduler.Current); // Default Scheduler
            Bind<IScheduler>().ToConstant(DispatcherScheduler.Current).WhenTargetHas<DispatcherAttribute>();
            Bind<IScheduler>().ToConstant(ThreadPoolScheduler.Instance).WhenTargetHas<ThreadPoolAttribute>();
            Bind<IMessageBus>().ToConstant(MessageBus.Current);

            Bind<ICurrentProfile>().To<ProfileService>().InSingletonScope();
        }
    }

    class ServiceModule : Ninject.Modules.NinjectModule
    {
        private void BindAndActivateSingleton<T>()
        {
            Bind<T>().ToSelf().InSingletonScope();
            Kernel.Get<T>();
        }

        public override void Load()
        {   
            Bind<INotificationService>().To<NotificationService>().InSingletonScope();           

            Bind<SettingsService>().ToSelf().InSingletonScope();
            Bind<ISettingsService>().ToConstant(Kernel.Get<SettingsService>());
            Bind<ICredentialsService>().ToConstant(Kernel.Get<SettingsService>());

            BindAndActivateSingleton<NavigationService>();

            Bind<IConnectivityService>().To<ConnectivityService>().InSingletonScope();

            Bind<IStoreImages>().To<MultimediaStorageService>().InSingletonScope();
            Bind<IStoreMultimedia>().ToConstant(Kernel.Get<IStoreImages>());
            Bind<FieldDataService>().ToSelf().InSingletonScope();
            Bind<IFieldDataService>().ToConstant(Kernel.Get<FieldDataService>());
            Bind<IKeyMappingService>().ToConstant(Kernel.Get<FieldDataService>());

            Bind<IVocabularyService>().To<VocabularyService>().InSingletonScope();
            Bind<ITaxonService>().To<TaxonService>().InSingletonScope();
            Bind<IMapStorageService>().To<MapStorageService>().InSingletonScope();
            Bind<IMapTransferService>().To<MapTransferService>().InSingletonScope();

            Bind<IDiversityServiceClient>().To<DiversityServiceClient>().InSingletonScope();

            Bind<CloudStorageService>().ToSelf().InSingletonScope();
            Bind<ICloudStorageService>().ToConstant(Kernel.Get<CloudStorageService>());

            var location = Kernel.Get<LocationService>();

            Kernel.Get<ISettingsService>()
                .SettingsObservable()
                .Where(s => s != null)
                .Select(s => s.UseGPS)
                .Subscribe(gps => location.IsEnabled = gps);

            Bind<ILocationService>().ToConstant(location);

            Bind<IRefreshVocabularyTask>().To<RefreshVocabularyTask>();

            Bind<ICleanupData>().To<CleanupService>();

            Bind<IBackupService>().To<BackupService>();
        }
    }
}
