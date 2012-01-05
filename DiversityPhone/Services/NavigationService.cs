using System;
using System.Net;
using Microsoft.Phone.Controls;
using ReactiveUI;
using DiversityPhone.Messages;
using DiversityPhone.Model;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Navigation;
using DiversityPhone.ViewModels;


namespace DiversityPhone.Services
{
    public class NavigationService : INavigationService
    {
        private IMessageBus _messenger;
        private IList<IDisposable> _subscriptions;

        public NavigationService(IMessageBus messenger)
        {
            _messenger = messenger;    
       
            _subscriptions = new List<IDisposable>()
            {
                _messenger.Listen<Page>()
                    .Subscribe(p => Navigate(p,null,null)),
                _messenger.Listen<Message>()
                    .Subscribe(m =>
                        {
                            switch (m)
                            {                            
                                case Message.NavigateBack:
                                    NavigateBack();
                                    break;
                                case Message.ClearHistory:
                                    ClearHistory();
                                    break;
                                default:
                                    break;
                            }
                        }),                

                _messenger.Listen<NavigationMessage>()
                    .Subscribe(msg => Navigate(msg.Destination,msg.Context, msg.Referrer)),
            };            
        }
        public void AttachToNavigation(PhoneApplicationFrame frame)
        {
            if (frame != null)
            {
                frame.Navigating += RootFrame_Navigating;
                frame.FragmentNavigation += new FragmentNavigationEventHandler(frame_FragmentNavigation);                
            }
        }

        void frame_FragmentNavigation(object sender, FragmentNavigationEventArgs e)
        {
            var page = App.RootFrame.Content as PhoneApplicationPage;
            var token = e.Fragment;
            PageState storedState = null;
            if (token != null)
            {
                App.StateTracker.TryGetValue(token, out storedState);                
            }
            
            if (storedState != null && page != null && page.DataContext is PageViewModel)
            {
                var vm = page.DataContext as PageViewModel;
                vm.SetState(storedState);
            }
        }
        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var page = App.RootFrame.Content as PhoneApplicationPage;
            if (page != null && page.DataContext is PageViewModel)
            {
                var vm = page.DataContext as PageViewModel;
                vm.SaveState();
            }


        }
        public void Navigate(Page p, string context, string referrer)
        {
            string destination = null;
            switch (p)
            {
                case Page.Home:
                    destination = "/View/Home.xaml";
                    break;
                case Page.Settings:
                    destination = "/View/Settings.xaml";
                    break;
                case Page.EditES:
                    destination = "/View/EditES.xaml";
                    break;
                case Page.ViewES:
                    destination = "/View/ViewES.xaml";
                    break;
                case Page.EditEV:
                    destination = "/View/EditEV.xaml";
                    break;
                case Page.ViewEV:
                    destination = "/View/ViewEV.xaml";
                    break;
                case Page.EditIU:
                    destination = "/View/EditIU.xaml";
                    break;
                case Page.ViewIU:
                    destination = "/View/ViewIU.xaml";
                    break;
                case Page.EditCS:
                    destination = "/View/EditCS.xaml";
                    break;
                case Page.ViewCS:
                    destination = "/View/ViewCS.xaml";
                    break;
                case Page.LoadedMaps:
                    destination = "/View/ViewLM.xaml";
                    break;
                case Page.DownLoadMaps:
                    destination = "/View/ViewDLM.xaml";
                    break;


#if DEBUG
                default:
                    throw new NotImplementedException();
#endif
            }
            if (destination != null && App.RootFrame != null)
            {
                string token = Guid.NewGuid().ToString();
                Uri uri = new Uri(String.Format("{0}#{1}", destination, token), UriKind.Relative);
                App.StateTracker.Add(token, new PageState(token, context, referrer));

                App.RootFrame.Navigate(uri);
            }
        }        

        public bool CanNavigateBack()
        {
            return App.RootFrame.CanGoBack;
        }

        public void NavigateBack()
        {
            App.RootFrame.GoBack();
        }        

        public void ClearHistory()
        {
            while (App.RootFrame.CanGoBack)
                App.RootFrame.RemoveBackEntry();
        }
    }
}
