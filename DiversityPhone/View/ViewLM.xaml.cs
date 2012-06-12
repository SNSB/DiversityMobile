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

using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;


namespace DiversityPhone.View
{
    public partial class ViewMapPicker : PhoneApplicationPage
    {
        private ViewMapPickerVM VM { get { return DataContext as ViewMapPickerVM; } }  
        
        public ViewMapPicker()
        {
            InitializeComponent();
            //this.ProgressBar.Visibility = Visibility.Visible;
            //this.ProgressBar.IsIndeterminate = true;
            this._standardProgressBar.Visibility = Visibility.Collapsed;
            //this._standardProgressBar.IsIndeterminate = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.ProgressBar.Visibility = Visibility.Collapsed;
            this.ProgressBar.IsIndeterminate = false;
        }

        private void LoadMaps_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.AddMaps.Execute(null);
        }


        private void ListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //Test for ProgreesBinding Here
            this.ProgressBar.Visibility = Visibility.Visible;
            this.ProgressBar.IsIndeterminate = true;
        }

    }
}