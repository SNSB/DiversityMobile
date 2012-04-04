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
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

         
        }

        private void LoadMaps_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.AddMaps.Execute(null);
        }

    }
}