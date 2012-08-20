using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DiversityPhone.Services;
using System.Windows.Navigation;
using ReactiveUI;
using Funq;
using DiversityPhone.Services.BackgroundTasks;
using System.IO.IsolatedStorage;
using System.Device.Location;
using System;
using DiversityPhone.Messages;
using DiversityPhone.ViewModels;
using DiversityPhone.ViewModels.Maps;
using DiversityPhone.ViewModels.Utility;


namespace DiversityPhone
{
    public partial class App : Application
    {       
        private const string TASK_KEY = "BackgroundTasks";

        public static Container IOC { get; private set; }       
        
        private static BackgroundService BackgroundTasks;
        private static IMessageBus Messenger;
        private static Services.NavigationService NavSvc;
        private static ISettingsService Settings;
        private static ILocationService GeoLocation { get { return IOC.Resolve<ILocationService>(); } }
        public static IFieldDataService OfflineDB { get { return IOC.Resolve<IFieldDataService>(); } }
        
        
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

       

        private static void registerViewModels()
        {



            #region ViewModel Factories
            IOC.Register<HomeVM>(new HomeVM(IOC));
            IOC.Register<EditESVM>(new EditESVM(IOC));
            IOC.Register<ViewESVM>(new ViewESVM(IOC));
            IOC.Register<EditEVVM>(new EditEVVM(IOC));
            IOC.Register<ViewEVVM>(new ViewEVVM(IOC));
            IOC.Register<EditCSVM>(new EditCSVM());
            IOC.Register<ViewCSVM>(new ViewCSVM(IOC));
            IOC.Register<EditIUVM>(new EditIUVM(IOC));
            IOC.Register<ViewIUVM>(new ViewIUVM(IOC));
            IOC.Register<EditPropertyVM>(new EditPropertyVM(IOC));
            IOC.Register<EditAnalysisVM>(new EditAnalysisVM(IOC));

            IOC.Register<MapManagementVM>(new MapManagementVM(IOC));
            IOC.Register<ViewMapVM>(new ViewMapVM(IOC));
            
            IOC.Register<EditMapVM>(c => new EditMapVM(c.Resolve<IMessageBus>()));            
            IOC.Register<NewImageVM>(new NewImageVM());
            IOC.Register<NewAudioVM>(new NewAudioVM());
            IOC.Register<NewVideoVM>(new NewVideoVM());
            IOC.Register<ViewImageVM>(new ViewImageVM());
            IOC.Register<ViewAudioVM>(new ViewAudioVM());
            IOC.Register<ViewVideoVM>(new ViewVideoVM());
            

            IOC.Register<TaxonManagementVM>(c => new TaxonManagementVM(c));

            IOC.Register<SettingsVM>(c => new SettingsVM(c));

            IOC.Register<SyncVM>(c => new SyncVM(c));

            IOC.Register<SetupVM>(c => new SetupVM(c));
            #endregion
            
        }

        public static void Initialize()
        {           

            Messenger = MessageBus.Current;
            Settings = new SettingsService(Messenger);
            NavSvc = new Services.NavigationService(Messenger);

            NavSvc.AttachToNavigation(RootFrame);


            IOC = new Container();
            IOC.DefaultReuse = ReuseScope.None;

            

            IOC.Register<IMessageBus>(Messenger);
            IOC.Register<ISettingsService>(Settings);
            IOC.Register<Services.NavigationService>(NavSvc);

            IOC.Register<DialogService>(new DialogService(IOC.Resolve<IMessageBus>()));
            IOC.Register<IConnectivityService>(new ConnectivityService());
            IOC.Register<IFieldDataService>(new OfflineStorage(IOC.Resolve<IMessageBus>()));
            IOC.Register<ITaxonService>(new TaxonService());
            IOC.Register<IVocabularyService>(new VocabularyService(IOC.Resolve<IMessageBus>()));
            IOC.Register<IMapStorageService>(new MapStorageService(IOC.Resolve<IMessageBus>()));
            IOC.Register<IMapTransferService>(new MapTransferService());
            

            IOC.Register<IDiversityServiceClient>(new DiversityServiceObservableClient(IOC.Resolve<IMessageBus>()));
            IOC.Register<ILocationService>(new LocationService(IOC));
            IOC.Register<IMultiMediaClient>(new MultimediaClient(IOC.Resolve<ISettingsService>()));
            
            BackgroundTasks = new BackgroundService();
            BackgroundTasks.registerTask(new DownloadTaxonListTask(IOC));
            BackgroundTasks.registerTask(new RefreshVocabularyTask(IOC));
            BackgroundTasks.registerTask(new UploadEventTask(IOC));
            BackgroundTasks.registerTask(new UploadMultimediaTask(IOC));
            IOC.Register<IBackgroundService>(BackgroundTasks);
            restartBackgroundTasks();

            registerViewModels();

            RxApp.MessageBus.SendMessage(new EventMessage(), MessageContracts.INIT);

            NLog.LogManager.EnableLogging();
        }

        private static void restartBackgroundTasks()
        {
            object savedTasks = null;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(TASK_KEY, out savedTasks)
                && savedTasks != null
                && savedTasks is IEnumerable<BackgroundTaskInvocation>)
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(TASK_KEY);
                BackgroundTasks.setQueue(savedTasks as IEnumerable<BackgroundTaskInvocation>);
                BackgroundTasks.resume();
            }
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
            if (BackgroundTasks != null)
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(TASK_KEY); // Remove stored Tasks, they will resume automatically            
                BackgroundTasks.resume();
            }
            
            // Ensure that application state is restored appropriately
            
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here./          
            if (BackgroundTasks != null)
            {
                BackgroundTasks.suspend();
                IsolatedStorageSettings.ApplicationSettings[TASK_KEY] = BackgroundTasks.dumpQueue().ToList();
            }
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            if (BackgroundTasks != null)
            {
                BackgroundTasks.suspend();
                IsolatedStorageSettings.ApplicationSettings[TASK_KEY] = BackgroundTasks.dumpQueue().ToList();
            }
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
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

            if(Messenger != null)
                Messenger.SendMessage(new DialogMessage(DialogType.OK, "", string.Format("{0} {1}", DiversityResources.Message_FatalException, e.ExceptionObject.Message)));
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
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
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