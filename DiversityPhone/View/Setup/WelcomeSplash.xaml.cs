using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ninject;
using ReactiveUI;
using System.Linq;
using System.Windows;

namespace DiversityPhone.View.Setup
{
    public partial class WelcomeSplash : PhoneApplicationPage
    {

        private ApplicationBarIconButton OKNext;
        private ISettingsService Settings;
        private IMessageBus Messenger;

        public WelcomeSplash()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            ShowSplash();

            ClearBackStack();

            InitializeServicesIfNecessary();


            if (Settings.CurrentSettings == null) //No settings found -> enter first time setup
            {
                ShowSetup();
            }
            else // skip setup and enter main app
            {
                NavigateHome();
            }
        }

        private void NavigateHome()
        {
            Messenger.SendMessage(Page.Home);
        }

        private void InitializeServicesIfNecessary()
        {
            App.Initialize();
            Messenger = App.Kernel.Get<IMessageBus>();
            Settings = App.Kernel.Get<ISettingsService>();
        }

        private void ShowSetup()
        {
            OKNext = OKNext ?? new OKNextPageButton(Messenger, Page.SetupLogin);
            this.ApplicationBar.Buttons.Add(OKNext);
            SplashImage.Visibility = System.Windows.Visibility.Collapsed;
            SetupWelcome.Visibility = System.Windows.Visibility.Visible;
            this.ApplicationBar.IsVisible = true;
        }

        private void ClearBackStack()
        {
            while (this.NavigationService.BackStack.Any())
                this.NavigationService.RemoveBackEntry();
        }

        private void ShowSplash()
        {
            this.ApplicationBar.Buttons.Clear();
            SplashImage.Visibility = System.Windows.Visibility.Visible;
            SetupWelcome.Visibility = System.Windows.Visibility.Collapsed;
            this.ApplicationBar.IsVisible = false;
        }
    }
}