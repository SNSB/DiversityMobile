namespace DiversityPhone
{
    using DiversityPhone.Interface;
    using DiversityPhone.ViewModels;
    using Ninject;
    using ReactiveUI;

    class ViewModelModule : Ninject.Modules.NinjectModule
    {
        private void BindAndActivateSingleton<T>()
        {
            var tmp = Kernel.Get<T>();
            Bind<T>().ToConstant(tmp).InSingletonScope();
        }

        public override void Load()
        {
            PageVMBase.Initialize(Kernel.Get<IMessageBus>(), Kernel.Get<INotificationService>());

            BindAndActivateSingleton<HomeVM>();
            BindAndActivateSingleton<ViewESVM>();
            BindAndActivateSingleton<EditESVM>();
            BindAndActivateSingleton<ViewEVVM>();
            BindAndActivateSingleton<EditEVVM>();
            BindAndActivateSingleton<ViewCSVM>();
            BindAndActivateSingleton<EditCSVM>();
            BindAndActivateSingleton<ViewIUVM>();
            BindAndActivateSingleton<EditIUVM>();
            BindAndActivateSingleton<EditPropertyVM>();
            BindAndActivateSingleton<EditAnalysisVM>();

            BindAndActivateSingleton<MapManagementVM>();
            BindAndActivateSingleton<ViewMapVM>();

            BindAndActivateSingleton<ViewImageVM>();
            BindAndActivateSingleton<NewImageVM>();
            BindAndActivateSingleton<AudioVM>();
            BindAndActivateSingleton<VideoVM>();

            Bind<IUploadVM<IElementVM>>().To<FieldDataUploadVM>().InSingletonScope();
            Bind<IUploadVM<MultimediaObjectVM>>().To<MultimediaUploadVM>().InSingletonScope();
            BindAndActivateSingleton<UploadVM>();
            BindAndActivateSingleton<DownloadVM>();

            BindAndActivateSingleton<SettingsVM>();

            BindAndActivateSingleton<SetupVM>();

            Bind<TaxonManagementVM>().ToSelf().InTransientScope();

            BindAndActivateSingleton<ImportExportVM>();
        }

    }
}
