using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ninject;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;

namespace DiversityPhone.View.Setup
{
    public partial class WelcomeSplash : PhoneApplicationPage
    {
        private bool Initialized = false;
        private ApplicationBarIconButton OKNext;
        private ISettingsService Settings;
        private IMessageBus Messenger;

        public WelcomeSplash()
        {
            InitializeComponent();
            ShowSplash();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Initialized)
            {
                Initialized = true;

                ClearBackStack();

                InitializeServicesIfNecessary();

                var dispatcher = new DispatcherScheduler(Dispatcher);

                dispatcher.Schedule(dispatcher.Now.AddSeconds(2), CheckSettings);
            }
        }

        private void CheckSettings()
        {
            Settings.SettingsObservable()
                .Take(1)
                .Subscribe(s =>
                {
                    if (s == null) //No settings found -> enter first time setup
                    {
                        ShowSetup();
                    }
                    else // skip setup and enter main app
                    {
                        NavigateHome();
                    }
                });
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



        private void MapWiki_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Map_URL, UriKind.Absolute)
            }.Show();
        }

        private void TaxWiki_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Taxa_URL, UriKind.Absolute)
            }.Show();
        }

        private void Mail_Click(object sender, RoutedEventArgs e)
        {
            new EmailComposeTask()
            {
                To = DiversityResources.App_Mail_Address,
                Subject = "DiversityMobile"
            }.Show();
        }

        private void Homepage_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Homepage_URL, UriKind.Absolute)
            }.Show();
        }
    }
}