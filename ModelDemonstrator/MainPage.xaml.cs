using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;


namespace ModelDemonstrator
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            //this.image1.Source = bi;
            //this.listBox1.FontSize = 32;
            //this.listBox1.Items.Add("No EventSeries");
            //this.image2.Source = bi;
            //this.textBlock1.Text = "No Event Series";
            //this.listBox1.Items.Add(canvas1);

        }

        private void canvas1_Tap(object sender, GestureEventArgs e)
        {
            MessageBox.Show("Tapped");
        }

        private void canvas2_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EventSeries.xaml",UriKind.Relative));
        }

        private void canvas3_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EventSeries.xaml", UriKind.Relative));
        }

        private void Add_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ViewES.xaml", UriKind.Relative));
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
    }
}