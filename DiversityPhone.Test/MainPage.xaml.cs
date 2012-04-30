using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;

namespace DiversityPhone.Test
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = false;

            

            var testPage = (IMobileTestPage)UnitTestSystem.CreateTestPage();
            BackKeyPress += (x, xe) => xe.Cancel = testPage.NavigateBack();
            ((PhoneApplicationFrame)Application.Current.RootVisual).Content = testPage;
        }
    }
}