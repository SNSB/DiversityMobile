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
        private static IGeoLocationService GeoLocation { get { return IOC.Resolve<IGeoLocationService>(); } }
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
            IOC.Register<HomeVM>(new HomeVM(
                IOC.Resolve<IMessageBus>(),
                IOC.Resolve<IFieldDataService>(),
                IOC.Resolve<IDiversityServiceClient>(),
                IOC.Resolve<ISettingsService>()
                ));

            IOC.Register<EditESVM>(c => new EditESVM(c.Resolve<ISettingsService>()));

            IOC.Register<ViewESVM>(container => new ViewESVM());
            IOC.Register<EditEVVM>(c => new EditEVVM(c));


            IOC.Register<ViewEVVM>(c => new ViewEVVM());

            IOC.Register<ViewCSVM>(c => new ViewCSVM(c));
            IOC.Register<EditCSVM>(c => new EditCSVM());
            IOC.Register<ViewMapPickerVM>(c => new ViewMapPickerVM(c.Resolve<IMapStorageService>()));
            IOC.Register<ViewDLMVM>(c => new ViewDLMVM(c.Resolve<IMapTransferService>()));
            IOC.Register<ViewDownloadMapsVM>(c => new ViewDownloadMapsVM(c.Resolve<IMapStorageService>()));
            IOC.Register<ViewMapVM>(c => new ViewMapVM(c.Resolve<IMapStorageService>(), c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            IOC.Register<ViewMapESVM>(c => new ViewMapESVM(c.Resolve<IMapStorageService>(), c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            IOC.Register<ViewMapEVVM>(c => new ViewMapEVVM(c.Resolve<IMapStorageService>(), c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            IOC.Register<ViewMapIUVM>(c => new ViewMapIUVM(c.Resolve<IMapStorageService>(), c.Resolve<IGeoLocationService>(), c.Resolve<ISettingsService>()));
            IOC.Register<EditIUVM>(c => new EditIUVM(c));
            IOC.Register<ViewIUVM>(c => new ViewIUVM(c));
            IOC.Register<EditAnalysisVM>(c => new EditAnalysisVM(c));
            IOC.Register<EditMapVM>(c => new EditMapVM(c.Resolve<IMessageBus>()));
            IOC.Register<SelectNewMMOVM>(c => new SelectNewMMOVM());
            IOC.Register<NewImageVM>(c => new NewImageVM());
            IOC.Register<NewAudioVM>(c => new NewAudioVM());
            IOC.Register<NewVideoVM>(c => new NewVideoVM());
            IOC.Register<ViewImageVM>(c => new ViewImageVM());
            IOC.Register<ViewAudioVM>(c => new ViewAudioVM());
            IOC.Register<ViewVideoVM>(c => new ViewVideoVM());
            IOC.Register<EditPropertyVM>(c => new EditPropertyVM(c));

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

            IOC.Register<IFieldDataService>(new OfflineStorage(IOC.Resolve<IMessageBus>()));
            IOC.Register<ITaxonService>(new TaxonService());
            IOC.Register<IVocabularyService>(new VocabularyService(IOC.Resolve<IMessageBus>()));
            IOC.Register<IMapStorageService>(new MapStorageService(IOC.Resolve<IMessageBus>()));
            IOC.Register<IMapTransferService>(new MapTranferService(IOC.Resolve<IMessageBus>()));
            

            IOC.Register<IDiversityServiceClient>(new DiversityServiceObservableClient(IOC.Resolve<IMessageBus>()));
            IOC.Register<IGeoLocationService>(new GeoLocationService(IOC.Resolve<IMessageBus>(), IOC.Resolve<ISettingsService>()));
            IOC.Register<IMultiMediaClient>(new MultimediaClient(IOC.Resolve<ISettingsService>()));
            
            BackgroundTasks = new BackgroundService();
            BackgroundTasks.registerTask(new DownloadTaxonListTask(IOC));
            BackgroundTasks.registerTask(new RefreshVocabularyTask(IOC));
            BackgroundTasks.registerTask(new UploadEventTask(IOC));
            BackgroundTasks.registerTask(new UploadMultimediaTask(IOC));
            IOC.Register<IBackgroundService>(BackgroundTasks);
            restartBackgroundTasks();

            registerViewModels();

            var settings = Settings.getSettings();

            if (settings != null && settings.UseGPS == true)
            {
                GeoLocation.startWatcher();
                if (settings.CurrentSeriesID != null)
                    GeoLocation.setTourEventSeriesID((int)settings.CurrentSeriesID);
            }

            RxApp.MessageBus.SendMessage(new InitMessage());

            NLog.LogManager.Configuration.AddTarget("debug", new NLog.Targets.DebuggerTarget());
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




        #region Georeferencing

        public static GeoCoordinateWatcher Watcher;
        private static IList<Model.GeoPointForSeries> coordinates = new List<Model.GeoPointForSeries>();
        private static int? CurrentSeriesID = null;

        public static void startWatcher()
        {
            Watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            Watcher.MovementThreshold = 20;
            Watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            Watcher.Start();
        }

        public static void stopWatcher()
        {
            if (Watcher != null)
            {
                storeGeoPoints();
                Watcher = null;
            }
        }

        public static void startTourWhenGPSUsed(int SeriesID)
        {
            Model.AppSettings set = Settings.getSettings();
            set.CurrentSeriesID = SeriesID;
            Settings.saveSettings(set);
            CurrentSeriesID = SeriesID;
            coordinates = new List<Model.GeoPointForSeries>();
            if (set.UseGPS == true)
            {
                Model.GeoPointForSeries gp = new Model.GeoPointForSeries();
                gp.SeriesID = SeriesID;
                gp.Latitude = Watcher.Position.Location.Latitude;
                gp.Longitude = Watcher.Position.Location.Longitude;
                gp.Altitude = Watcher.Position.Location.Altitude;
                coordinates.Add(gp);
            }
        }

        public static void stopTour()
        {
            Model.AppSettings set = Settings.getSettings();
            set.CurrentSeriesID = null;
            Settings.saveSettings(set);
            CurrentSeriesID = null;
            storeGeoPoints();
        }

        static void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (Watcher == null)
                return;
            if (CurrentSeriesID != null)
            {
                Model.GeoPointForSeries newGeoPoint = new Model.GeoPointForSeries();
                newGeoPoint.Latitude = e.Position.Location.Latitude;
                newGeoPoint.Longitude = e.Position.Location.Longitude;
                newGeoPoint.Altitude = e.Position.Location.Altitude;
                coordinates.Add(newGeoPoint);
                if (coordinates.Count >= 10)
                {
                    storeGeoPoints();
                }
            }
        }

        internal static void storeGeoPoints()
        {
            foreach (Model.GeoPointForSeries gp in coordinates)
                OfflineDB.addOrUpdateGeoPoint(gp);
            coordinates = new List<Model.GeoPointForSeries>();
        }

        public void fillGeoCoordinates(Model.ILocalizable loc)
        {
            if (Watcher != null && Watcher.Status == GeoPositionStatus.Ready)
            {
                var geoPos = Watcher.Position;
                loc.Altitude = geoPos.Location.Altitude;
                loc.Latitude = geoPos.Location.Latitude;
                loc.Longitude = geoPos.Location.Longitude;

            }
            else
            {
                loc.Altitude = null;
                loc.Latitude = null;
                loc.Longitude = null;
            }
        }

        #endregion
    }
}