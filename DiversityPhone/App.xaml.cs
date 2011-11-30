﻿using System;
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
using Wintellect.Sterling;
using System.Reactive.Subjects;
using System.Windows.Navigation;
using DiversityPhone.Service;
using ReactiveUI;
using System.Reactive.Concurrency;


namespace DiversityPhone
{
    public partial class App : Application
    {
        public static IOfflineStorage OfflineDB { get; private set; } 
       
        public static IDiversityService Repository { get; private set; }
        
        private static SterlingEngine _engine;
        private static SterlingDefaultLogger _logger;            

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



        public static IDictionary<string, PageState> StateTracker { get; private set; }
        
        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;           
            
            Repository = new DiversityServiceClient(); //Push Nav-Service

            OfflineDB = new OfflineStorage(MessageBus.Current);

            StateTracker = new Dictionary<string, PageState>();

            // Standard Silverlight initialization
            InitializeComponent();
            

            // Phone-specific initialization
            InitializePhoneApplication();

            NavSvc.AttachToNavigation(RootFrame);

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }


            
        }
       

        public static void _ActivateEngine()
        {
            _engine = new SterlingEngine();
            _logger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
            _engine.Activate();
            //_offlineStorage.OnNext(new OfflineStorage(_engine.SterlingDatabase.RegisterDatabase<DiversityDatabase>()));           


            //_database.RegisterTrigger<ItemViewModel, int>(new WindowsPhoneSterling.Sterling.ISODatabase.ItemTrigger(maxIdx));

        }

        public static void _DeactivateEngine()
        {            
            _logger.Detach();
            _engine.Dispose();
            //_offlineStorage.OnNext(null);
            _engine = null;
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            _ActivateEngine();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            _ActivateEngine();
            // Ensure that application state is restored appropriately
            
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
            _DeactivateEngine();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            _DeactivateEngine();
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