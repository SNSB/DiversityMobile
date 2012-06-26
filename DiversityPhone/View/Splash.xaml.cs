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
using DiversityPhone.Services;

namespace DiversityPhone.View
{
    public partial class Splash : PhoneApplicationPage
    {


        public Splash()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Initialize();
            var settings = App.IOC.Resolve<ISettingsService>().getSettings();
            var navigation = App.IOC.Resolve<NavigationService>();
            if (settings == null)
                this.NavigationService.Navigate(new Uri("/View/Setup.xaml", UriKind.RelativeOrAbsolute));
            else
                this.NavigationService.Navigate(new Uri("/View/Home.xaml", UriKind.RelativeOrAbsolute));

        }
    }
}