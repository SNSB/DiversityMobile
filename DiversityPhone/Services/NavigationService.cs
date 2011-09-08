using System;
using System.Net;
using Microsoft.Phone.Controls;
using ReactiveUI;
using DiversityPhone.Messages;
using DiversityPhone.Model;
using System.Collections;
using System.Collections.Generic;


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
                    .Subscribe(_=>Navigate(Page.EditEventSeries)),
                _messenger.Listen<EventSeries>(MessageContracts.SELECT)
                    .Subscribe(_=>Navigate(Page.ViewEventSeries)),

                _messenger.Listen<Event>(MessageContracts.EDIT)
                    .Subscribe(_=>Navigate(Page.EditEvent)),
                _messenger.Listen<Event>(MessageContracts.SELECT)
                    .Subscribe(_=>Navigate(Page.ViewEent)),

                _messenger.Listen<IdentificationUnit>(MessageContracts.EDIT)
                    .Subscribe(_=>Navigate(Page.EditIUnit)),
                _messenger.Listen<IdentificationUnit>(MessageContracts.SELECT)
                    .Subscribe(_=>Navigate(Page.ViewIUnit)),
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
                case Page.Setup:
                    destination = new Uri("/View/FirstTimeSetup.xaml", UriKind.Relative);
                    break;
                case Page.Upload:
                    destination = new Uri("/View/Upload.xaml", UriKind.Relative);
                    break;
                case Page.ListEventSeries:
                    destination = new Uri("/View/ListES.xaml", UriKind.Relative);
                    break;                
                case Page.EditEventSeries:
                    destination = new Uri("/View/EditES.xaml", UriKind.Relative);
                    break;
                case Page.ViewEventSeries:
                    destination = new Uri("/View/ViewES.xaml", UriKind.Relative);
                    break;
                case Page.EditEvent:
                    destination = new Uri("/View/EditEV.xaml", UriKind.Relative);
                    break;   
                case Page.ViewEent:
                    destination = new Uri("/View/ViewEV.xaml", UriKind.Relative);
                    break;
                case Page.EditIUnit:
                    destination = new Uri("/View/EditIU.xaml", UriKind.Relative);
                    break;
                case Page.ViewIUnit:
                    destination = new Uri("/View/ViewIU.xaml", UriKind.Relative);
                    break;

#if DEBUG
                default:
                    throw new NotImplementedException();
#endif
            }
            if (destination != null && App.RootFrame != null)
                App.RootFrame.Navigate(destination);
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
