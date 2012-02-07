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
using DiversityPhone.ViewModels;



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
       

        private void GetVoc_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.GetVocabulary.Execute(null);
        }

        private void UploadMMO_Click(object sender, EventArgs e)
        {
            if (VM != null) ;
                //VM.UploadMMO.Execute(null);
        }

        private void Upload_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Upload.Execute(null);
        }

        private void LoadedMaps_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Maps.Execute(null);
        }
    }
}