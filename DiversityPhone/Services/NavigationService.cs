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
                    .Subscribe(p => Navigate(p)),
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

                _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                    .Subscribe(_=>Navigate(Page.EditES)),
                _messenger.Listen<EventSeries>(MessageContracts.SELECT)
                    .Subscribe(_=>Navigate(Page.ViewES)),

                _messenger.Listen<Event>(MessageContracts.EDIT)
                    .Subscribe(_=>Navigate(Page.EditEV)),
                _messenger.Listen<Event>(MessageContracts.SELECT)
                    .Subscribe(_=>Navigate(Page.ViewEV)),

                _messenger.Listen<IdentificationUnit>(MessageContracts.EDIT)
                    .Subscribe(_=>Navigate(Page.EditIU)),
                _messenger.Listen<IdentificationUnit>(MessageContracts.SELECT)
                    .Subscribe(_=>Navigate(Page.ViewIU)),

                _messenger.Listen<NavigationMessage>()
                    .Subscribe(msg => Navigate(msg)),
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
        public void Navigate(NavigationMessage msg)
        {
            string destination = null;
            switch (msg.Destination)
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



#if DEBUG
                default:
                    throw new NotImplementedException();
#endif
            }
            if (destination != null && App.RootFrame != null)
            {
                string token = Guid.NewGuid().ToString();
                Uri uri = new Uri(String.Format("{0}#{1}", destination, token), UriKind.Relative);
                App.StateTracker.Add(token, new PageState(token, msg.Context));

                App.RootFrame.Navigate(uri);
            }
        }

        public void Navigate(Page p)
        {
            //Don't use this overload any more
            System.Diagnostics.Debugger.Break();


            Navigate(new NavigationMessage(p, null));
        }

        public bool CanNavigateBack()
        {
            return App.RootFrame.CanGoBack;
        }

        public void NavigateBack()
        {
            App.RootFrame.GoBack();
        }

        public static void ClearHistoryUntilCurrentPage()
        {
            Uri thisPage=App.RootFrame.Source;
            IEnumerable<JournalEntry> stack = App.RootFrame.BackStack;
            IEnumerator<JournalEntry> loop = App.RootFrame.BackStack.GetEnumerator();
            loop.Reset();
            int pos = -1;
            int count = -1;
            while (loop.MoveNext())
            {
                count++;
                if (loop.Current.Source.Equals(thisPage))
                    pos = count;
            }
            if (pos >= 0)
            {
                int backsteps = count - pos;
                for (int i = 0; i <= backsteps; i++)
                    App.RootFrame.RemoveBackEntry();
            }
            else
            {
                //throw new Exception("Page not found");
            }
                
        }

        public void ClearHistory()
        {
            while (App.RootFrame.CanGoBack)
                App.RootFrame.RemoveBackEntry();
        }
    }
}
