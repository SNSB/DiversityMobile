using System;
using System.Net;
using Microsoft.Phone.Controls;


namespace DiversityPhone.Services
{
    public class NavigationService : INavigationService
    {
        private PhoneApplicationFrame phoneApplicationFrame;        

        public NavigationService(PhoneApplicationFrame phoneApplicationFrame)
        {
            // TODO: Complete member initialization
            this.phoneApplicationFrame = phoneApplicationFrame;
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
#if DEBUG
                default:
                    throw new NotImplementedException();
#endif
            }
            if (destination != null)
                phoneApplicationFrame.Navigate(destination);
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
