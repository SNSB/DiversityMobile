
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

                _add = new CommandButtonAdapter(VM.Add, ApplicationBar.Buttons[0] as IApplicationBarIconButton);
            }
        }
    }
}