
using DiversityPhone.ViewModels;
using ReactiveUI;
using DiversityPhone.Services;
using System.Windows;
using System;
using Microsoft.Phone.Controls;



namespace DiversityPhone
{
    public partial class Home : PhoneApplicationPage
    {
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

        private void AddSeries_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Add.Execute(null);
        }     

        private void UploadMMO_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.UploadMMO.Execute(null);
        }

        private void Upload_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.UploadPlain.Execute(null);
        }

        private void LoadedMaps_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Maps.Execute(null);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {            
            if (App.Settings.getSettings() == null)
                MessageBus.Current.SendMessage<Page>(Page.Settings);
            else
            {
                Splash.Visibility = Visibility.Collapsed;
                ApplicationBar.IsVisible = true;
            }
        }
    }
}