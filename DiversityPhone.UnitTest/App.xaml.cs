using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows;
using System.Windows.Navigation;

namespace DiversityPhone.UnitTest
{
    public partial class App
    {
        public PhoneApplicationFrame RootFrame { get; private set; }

        public App()
        {
            UnhandledException += ApplicationUnhandledException;
            InitializeComponent();
            InitializePhoneApplication();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Current.Host.Settings.EnableFrameRateCounter = true;
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
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

        private static void ApplicationUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        private bool phoneApplicationInitialized;

        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
            phoneApplicationInitialized = true;
        }

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }
    }
}