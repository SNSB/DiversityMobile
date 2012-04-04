using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;


using System.Windows.Resources;
using System.Windows.Media.Imaging;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using System.IO;

namespace DiversityPhone.View
{
    public partial class ViewDLM : PhoneApplicationPage
    {

        private ViewDownloadMapsVM VM { get { return DataContext as ViewDownloadMapsVM; } }

        public ViewDLM()
        {
            InitializeComponent();  
        }

        #region UI-Events
        private void mapText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            String s = ((TextBlock)sender).Tag as string;
            //TODO
            //Show Details of the Map-Requires XML-Data for the Map
        }

     
        #endregion
      
        private void btn_Search_Click(object sender, RoutedEventArgs e)
        {

            if (textBoxSearch.Text.Length > 2)
                VM.Search.Execute(textBoxSearch.Text);
            else
                MessageBox.Show("Minimum lenght for search is 3");
        }

        private void btn_Show_Click(object sender, RoutedEventArgs e)
        {
            VM.Load.Execute(null);
        }

       
        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (VM != null)
                e.Cancel = VM.IsBusy;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if(VM!=null)
                VM.Search.Execute(((Button) sender).Tag as string);
        }

    }
}