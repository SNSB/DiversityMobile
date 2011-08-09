using System;
using System.Net;
using Microsoft.Phone.Controls;


namespace DiversityPhone.Services
{
    public class NavigationService : INavigationService
    {             

        public NavigationService()
        {
           
        }

        public void Navigate(Page p)
        {
            Uri destination = null;
            switch (p)
            {
                case Page.Home:
                    destination = new Uri("/Home.xaml",UriKind.Relative);
                    break;
                case Page.EventSeries:
                    destination = new Uri("/EventSeries.xaml", UriKind.Relative);
                    break;
                case Page.Settings:
                    destination = new Uri("/Settings.xaml", UriKind.Relative);
                    break;
                case Page.EditEventSeries:
                    destination = new Uri("/EditES.xaml", UriKind.Relative);
                    break;
#if DEBUG
                default:
                    throw new NotImplementedException();
#endif
            }
            if (destination != null)
                App.RootFrame.Navigate(destination);
        }

        public bool CanNavigateBack()
        {
            throw new NotImplementedException();
        }

        public void NavigateBack()
        {
            throw new NotImplementedException();
        }
    }
}
