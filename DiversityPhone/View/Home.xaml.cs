using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Windows;

namespace DiversityPhone
{
    public partial class Home : PhoneApplicationPage
    {
        private CommandButtonAdapter _add;

        private HomeVM VM { get { return DataContext as HomeVM; } }

        public Home()
        {
            InitializeComponent();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Settings.Execute(null);
        }

        private void LoadedMaps_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Maps.Execute(null);
        }

        private bool initialized = false;

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            while (this.NavigationService.BackStack.Any())
                this.NavigationService.RemoveBackEntry();

            if (!initialized)
            {
                initialized = true;

                var add =
                    new ApplicationBarIconButton(new Uri("/Images/appbar.add.rest.png", UriKind.RelativeOrAbsolute))
                    {
                        Text = DiversityResources.Home_Header_ButtonAdd
                    };
                ApplicationBar.Buttons.Add(add);
                _add = new CommandButtonAdapter(add, VM.Add);

                var settings =
                    new ApplicationBarIconButton(new Uri("/Images/appbar.feature.settings.rest.png", UriKind.RelativeOrAbsolute))
                    {
                        Text = DiversityResources.Home_Header_ButtonSettings
                    };
                settings.Click += Settings_Click;
                ApplicationBar.Buttons.Add(settings);

                var maps =
                    new ApplicationBarIconButton(new Uri("/Images/appbar.globe.rest.png", UriKind.RelativeOrAbsolute))
                    {
                        Text = DiversityResources.Home_Header_ButtonMaps
                    };
                maps.Click += LoadedMaps_Click;
                ApplicationBar.Buttons.Add(maps);

                var help =
                    new ApplicationBarIconButton(new Uri("/Images/appbar.questionmark.rest.png", UriKind.RelativeOrAbsolute))
                    {
                        Text = DiversityResources.Home_Header_Help
                    };
                help.Click += help_Click;
                ApplicationBar.Buttons.Add(help);
            }
        }

        private void help_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Help.Execute(null);
        }
    }
}