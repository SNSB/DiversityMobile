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

namespace DiversityPhone.View
{
    public partial class ViewDownloadMaps : PhoneApplicationPage
    {

        private ViewDownloadMapsVM VM { get { return DataContext as ViewDownloadMapsVM; } }
        private ProgressBinding<ViewDownloadMapsVM> _progress;

        public ViewDownloadMaps()
        {
            InitializeComponent();
            _progress = new ProgressBinding<ViewDownloadMapsVM>(VM, x => x.IsBusy);
        }

        private void btn_Search_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxSearch.Text.Length > 2)
                VM.Search.Execute(textBoxSearch.Text);
            else
                MessageBox.Show("Minimum search text length is 3!");
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            VM.Add.Execute(((Button)sender).Tag as String);
        }


        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = VM.IsBusy;
        }

        private void mapText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            String s = ((TextBlock)sender).Tag as string;
            //TODO
            //Show Details of the Map-Requires XML-Data for the Map
        }
    }
}