using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace DiversityPhone.View
{
    public partial class Info : PhoneApplicationPage
    {
        public Info()
        {
            InitializeComponent();
        }

        private void MapWiki_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Map_URL, UriKind.Absolute)
            }.Show();
        }

        private void Mail_Click(object sender, RoutedEventArgs e)
        {
            new EmailComposeTask()
            {
                To = DiversityResources.App_Mail_Address,
                Subject = "DiversityMobile"
            }.Show();
        }

        private void Homepage_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Homepage_URL, UriKind.Absolute)
            }.Show();
        }
    }
}