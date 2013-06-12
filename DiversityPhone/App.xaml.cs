using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DiversityPhone.Services;
using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.ViewModels.Utility;
using System.Reactive.Concurrency;
using Ninject;
using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.Services.BackgroundTasks;
using System;
using Ninject.Extensions.Factory;


namespace DiversityPhone
{
    public partial class App : Application
    {
        private const string TASK_KEY = "BackgroundTasks";

        public static IKernel Kernel { get; private set; }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;



            // Standard Silverlight initialization
            InitializeComponent();


            // Phone-specific initialization
            InitializePhoneApplication();


            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualizat*ion = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        class ViewModelModule : Ninject.Modules.NinjectModule
        {
            private void BindAndActivateSingleton<T>()
            {
                var tmp = Kernel.Get<T>();
                Bind<T>().ToConstant(tmp).InSingletonScope();
            }

            public override void Load()
            {
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
                Bind<IScheduler>().ToConstant(DispatcherScheduler.Current).WhenTargetHas<DispatcherAttribute>();
                Bind<IScheduler>().ToConstant(ThreadPoolScheduler.Instance).WhenTargetHas<ThreadPoolAttribute>();
                Bind<INotificationService>().To<NotificationService>().InSingletonScope();
                Bind<IMessageBus>().ToConstant(MessageBus.Current);

                Bind<SettingsService>().ToSelf().InSingletonScope();
                Bind<ISettingsService>().ToConstant(Kernel.Get<SettingsService>());
                Bind<ICurrentCredentials>().ToConstant(Kernel.Get<SettingsService>());

                BindAndActivateSingleton<NavigationService>();
                BindAndActivateSingleton<DialogService>();

                Bind<IConnectivityService>().To<ConnectivityService>().InSingletonScope();

                Bind<IStoreImages>().To<MultimediaStorageService>().InSingletonScope();
                Bind<IStoreMultimedia>().ToConstant(Kernel.Get<IStoreImages>());
                Bind<OfflineStorage>().ToSelf().InSingletonScope();
                Bind<IFieldDataService>().ToConstant(Kernel.Get<OfflineStorage>());
                Bind<IKeyMappingService>().ToConstant(Kernel.Get<OfflineStorage>());

                Bind<IVocabularyService>().To<VocabularyService>().InSingletonScope();
                Bind<ITaxonService>().To<TaxonService>().InSingletonScope();
                Bind<IMapStorageService>().To<MapStorageService>().InSingletonScope();
                Bind<IMapTransferService>().To<MapTransferService>().InSingletonScope();

                Bind<IDiversityServiceClient>().To<DiversityServiceClient>().InSingletonScope();

                var location = Kernel.Get<LocationService>();

                Kernel.Get<ISettingsService>()
                    .CurrentSettings()
                    .Where(s => s != null)
                    .Select(s => s.UseGPS)
                    .Subscribe(gps => location.IsEnabled = gps);

                Bind<ILocationService>().ToConstant(location);

                Bind<IRefreshVocabularyTask>().To<RefreshVocabularyTask>();
            }
        }


        class InitModule : Ninject.Modules.NinjectModule
        {
            public override void Load()
            {
                Kernel.Get<OfflineStorage>().CheckAndRepairDatabase();
                Kernel.Get<IMessageBus>().SendMessage(EventMessage.Default, MessageContracts.INIT);

            }
        }

        public static void Initialize()
        {
            //Work around an Issue with ReactiveUI
            //The WriteableBitmapEx library is recognized as the Pex Unit Test Runner
            RxApp.DeferredScheduler = null;
            RxApp.TaskpoolScheduler = null;
            RxApp.InUnitTestRunnerOverride = false;
            RxApp.DeferredScheduler = DispatcherScheduler.Current;
            RxApp.TaskpoolScheduler = ThreadPoolScheduler.Instance;


            Kernel = new StandardKernel();
            Kernel.Bind<PhoneApplicationFrame>().ToConstant(RootFrame);
            Kernel.Load<FuncModule>();
            Kernel.Load<ServiceModule>();
            Kernel.Load<ViewModelModule>();
            Kernel.Load<InitModule>();

        }




        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {

        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately

        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here./          

        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {

        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

            e.Handled = true;

            var messenger = Kernel.Get<IMessageBus>();
            if (messenger != null)
                messenger.SendMessage(new DialogMessage(DialogType.OK, "", string.Format("{0} {1}", DiversityResources.Message_FatalException, e.ExceptionObject.Message)));
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();

            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;



            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion





    }
}