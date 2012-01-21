using System;
using System.Linq;
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
    public class NavigationService
    {
        private IMessageBus _messenger;
        private IList<IDisposable> _subscriptions;
        private PhoneApplicationFrame _frame;

        private Stack<PageState> _States = null;
        public Stack<PageState> States 
        {
            get
            {
                return _States ?? (_States = new Stack<PageState>());
            }
            set
            {
                if (value != null)
                    _States = value;
            }
        }

        public NavigationService(IMessageBus messenger)
        {
            _messenger = messenger;                   
       
            _subscriptions = new List<IDisposable>()
            {
                _messenger.Listen<Page>()
                    .Subscribe(p => Navigate(new NavigationMessage(p,null))),
                _messenger.Listen<Message>()
                    .Subscribe(m =>
                        {
                            switch (m)
                            {                            
                                case Message.NavigateBack:
                                    System.Diagnostics.Debugger.Break();
                                    NavigateBack();
                                    break;                          
                                default:
                                    break;
                            }
                        }),                

                _messenger.Listen<NavigationMessage>()
                    .Subscribe(msg => Navigate(msg)),
            };            
        }
        public void AttachToNavigation(PhoneApplicationFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            _frame = frame;
            _frame.Navigating += RootFrame_Navigating;
            _frame.Navigated += RootFrame_Navigated; 
        }

        void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var page = _frame.Content as PhoneApplicationPage;       

            if (States.Any() && page != null && page.DataContext is PageViewModel)
            {
                var vm = page.DataContext as PageViewModel;
                vm.SetState(States.Peek());
            }
        }        
        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var page = App.RootFrame.Content as PhoneApplicationPage;

            if (page != null && page.DataContext is PageViewModel)
            {
                if(e.NavigationMode == NavigationMode.Back)
                {
                    States.Pop();
                }
                else
                {
                    var vm = page.DataContext as PageViewModel;
                    vm.SaveState();
                }
            }            
        }
        public void Navigate(NavigationMessage msg)
        {
            string destination = null;
            switch (msg.Destination)
            {
                case Page.Current:
                    return;
                case Page.Previous:
                    NavigateBack();
                    return;
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
                case Page.ViewMMO:
                    destination = "/View/ViewMMO.xaml";
                    break; 
                case Page.EditMMO:
                    destination = "/View/EditMMO.xaml";
                    break; 
                case Page.EditIUAN:
                    destination = "/View/EditAnalysis.xaml";
                    break;

#if DEBUG
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
#endif
            }
            if (destination != null && App.RootFrame != null)
            {
                States.Push(new PageState(msg.Context, msg.ReferrerType, msg.Referrer));

                App.RootFrame.Navigate(new Uri(destination,UriKind.RelativeOrAbsolute));
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
      
    }
}
