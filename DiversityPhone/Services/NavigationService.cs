using System;
using System.Net;
using Microsoft.Phone.Controls;
using ReactiveUI;
using DiversityPhone.Messages;
using DiversityPhone.Model;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Navigation;


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
            };


        }

        public void Navigate(Page p)
        {
            Uri destination = null;
            switch (p)
            {
                case Page.Home:
                    destination = new Uri("/View/Home.xaml",UriKind.Relative);
                    break;
                case Page.Settings:
                    destination = new Uri("/View/Settings.xaml", UriKind.Relative);
                    break;                
                case Page.EditES:
                    destination = new Uri("/View/EditES.xaml", UriKind.Relative);
                    break;
                case Page.ViewES:
                    destination = new Uri("/View/ViewES.xaml", UriKind.Relative);
                    break;
                case Page.EditEV:
                    destination = new Uri("/View/EditEV.xaml", UriKind.Relative);
                    break;   
                case Page.ViewEV:
                    destination = new Uri("/View/ViewEV.xaml", UriKind.Relative);
                    break;
                case Page.EditIU:
                    destination = new Uri("/View/EditIU.xaml", UriKind.Relative);
                    break;
                case Page.ViewIU:
                    destination = new Uri("/View/ViewIU.xaml", UriKind.Relative);
                    break;
                case Page.EditCS:
                    destination = new Uri("/View/EditCS.xaml", UriKind.Relative);
                    break;
                case Page.ViewCS:
                    destination = new Uri("/View/ViewCS.xaml", UriKind.Relative);
                    break;
                


#if DEBUG
                default:
                    throw new NotImplementedException();
#endif
            }
            if (destination != null && App.RootFrame != null)
            {
                App.RootFrame.Navigate(destination);
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

        public static void ClearHistoryUntilCurrentPage()
        {
            Uri actual=App.RootFrame.Source;
            IEnumerable<JournalEntry> stack = App.RootFrame.BackStack;
            IEnumerator<JournalEntry> loop = App.RootFrame.BackStack.GetEnumerator();
            loop.Reset();
            int pos = -1;
            int count = -1;
            while (loop.MoveNext())
            {
                count++;
                if (loop.Current.Source.Equals(actual))
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
