using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DiversityPhone.Services;
using System.Reactive.Subjects;
using System.Windows.Navigation;
using ReactiveUI;
using System.Reactive.Concurrency;
using DiversityPhone.ViewModels;
using System.Device.Location;
using System.Runtime.Serialization;


namespace DiversityPhone
{
    public partial class App : Application, IGeoLocationService
    {
        public static IFieldDataService OfflineDB { get; private set; }

        
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        private static DiversityPhone.Services.NavigationService _NavSvc = null;
        public static DiversityPhone.Services.NavigationService NavSvc
        {
            get
            {
                if (_NavSvc != null)
                    return _NavSvc;
                else
                    return _NavSvc = new Services.NavigationService(MessageBus.Current);
            }
        }

        private static ISettingsService _settings;
        public static ISettingsService Settings
        {
            get
            {
                return _settings ?? (_settings = new SettingsService(MessageBus.Current));
            }
        }



      
        
        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {            
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;            
            

            OfflineDB = new OfflineStorage(MessageBus.Current);                        

            // Standard Silverlight initialization
            InitializeComponent();
            

            // Phone-specific initialization
            InitializePhoneApplication();
            var settings = Settings.getSettings();
            if (settings != null && settings.UseGPS == true)
                startWatcher();

            if (settings != null)
                CurrentSeriesID = settings.CurrentSeries;

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
       

       

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            object savedStates = null;

            if (PhoneApplicationService.Current.State.TryGetValue("StateTracking", out savedStates)
                && savedStates != null
                && savedStates is IList<PageState>)
            {
                var stack = new Stack<PageState>((savedStates as IList<PageState>).Reverse());
                NavSvc.States = stack;
            }
                
            
            // Ensure that application state is restored appropriately
            
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here./
            PhoneApplicationService.Current.State["StateTracking"] = NavSvc.States.ToList();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            
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

            NavSvc.AttachToNavigation(RootFrame);

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

        public static GeoCoordinateWatcher Watcher = new GeoCoordinateWatcher();
        private static IList<Model.GeoPointForSeries> coordinates = new List<Model.GeoPointForSeries>();
        public static int? CurrentSeriesID = null;

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
                Watcher.Stop();
                storeGeoPoints();
            }
        }

        public static void startTourWhenGPSUsed(int SeriesID)
        {
            Model.AppSettings set = _settings.getSettings();
            set.CurrentSeries = SeriesID;
            _settings.saveSettings(set);
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

            Model.AppSettings set = _settings.getSettings();
            set.CurrentSeries = null;
            _settings.saveSettings(set);
            CurrentSeriesID = null;
            storeGeoPoints();
        }

        static void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
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
                OfflineDB.addOrUpdateGeopPoint(gp);
            coordinates = new List<Model.GeoPointForSeries>();
        }

        public void fillGeoCoordinates(Model.ILocalizable loc)
        {
            if (Watcher.Status==GeoPositionStatus.Ready)
            {
                var geoPos= Watcher.Position;
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