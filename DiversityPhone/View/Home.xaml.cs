
using DiversityPhone.ViewModels;
using ReactiveUI;
using DiversityPhone.Services;
using System.Windows;
using System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Reactive.Linq;
using DiversityPhone.Services.BackgroundTasks;
using DiversityPhone.View.Appbar;
using System.Linq;



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
                _add = new CommandButtonAdapter(VM.Add, ApplicationBar.Buttons[0] as IApplicationBarIconButton);
          
                var settings =
                    new ApplicationBarIconButton(new Uri("/Images/appbar.feature.settings.rest.png", UriKind.RelativeOrAbsolute))
                    {
                        Text = DiversityResources.Home_Header_ButtonSettings                   
                    };
                settings.Click += Settings_Click;
                ApplicationBar.Buttons.Add(settings);

                var maps =
                    new ApplicationBarIconButton(new Uri("/Images/appbar.map.rest.png", UriKind.RelativeOrAbsolute))
                    {
                        Text = DiversityResources.Home_Header_ButtonMaps                   
                    };
                maps.Click += LoadedMaps_Click;
                ApplicationBar.Buttons.Add(maps);
            }
        }
    }
}