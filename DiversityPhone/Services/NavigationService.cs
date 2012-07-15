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
using System.Reactive.Linq;


namespace DiversityPhone.Services
{
    public class NavigationService
    {
        private const string VISIT_KEY = "visit";        

        private IMessageBus _messenger;        
        private PhoneApplicationFrame _frame;
        private int visitCounter = 0;

        private IDictionary<string, PageState> States = new Dictionary<string, PageState>();

        
        public NavigationService(IMessageBus messenger)
        {
            _messenger = messenger;               
       
            
          _messenger.Listen<Page>()
                .Select(p => new NavigationMessage(p,null))
                .Merge(_messenger.Listen<NavigationMessage>())
                .Subscribe(msg => Navigate(msg));
                       
        }
        public void AttachToNavigation(PhoneApplicationFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            _frame = frame;
            _frame.Navigating += (s, args) => NavigationStarted();
                
            _frame.Navigated += (s, args) => NavigationFinished();
        }

        void NavigationFinished()
        {
            var page = _frame.Content as PhoneApplicationPage;
            string visit;
            PageState visit_info;
            if (page.NavigationContext.QueryString.TryGetValue(VISIT_KEY, out visit)
                && States.TryGetValue(visit, out visit_info)
                && page.DataContext is PageViewModel)
            {
                var vm = page.DataContext as PageViewModel;                
                vm.SetState(visit_info);
            }           
        }        
        void NavigationStarted()
        {
            var page = _frame.Content as PhoneApplicationPage;           

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
                case Page.Current:
                    return;
                case Page.Previous:
                    NavigateBack();
                    return;
                case Page.Home:
                    States.Clear();                    
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
                    //destination = "/View/ViewDLM.xaml";
                    destination = "/View/ViewDownloadMaps.xaml";
                    break;
                case Page.ViewMap:
                    destination = "/View/ViewMap.xaml";
                    break;
                case Page.ViewMapES:
                    destination = "/View/ViewMapES.xaml";
                    break;
                case Page.ViewMapEV:
                    destination = "/View/ViewMapEV.xaml";
                    break;
                case Page.ViewMapIU:
                    destination = "/View/ViewMapIU.xaml";
                    break;

                case Page.ViewImage:
                    destination = "/View/ViewImage.xaml";
                    break;
                case Page.ViewAudio:
                    destination = "/View/ViewAudio.xaml";
                    break;
                case Page.ViewVideo:
                    destination = "/View/ViewVideo.xaml";
                    break; 
                case Page.SelectNewMMO:
                    destination = "/View/SelectNewMMO.xaml";
                    break;
                case Page.NewImage:
                    destination = "/View/NewImage.xaml";
                    break;
                case Page.NewAudio:
                    destination = "/View/NewAudio.xaml";
                    break;
                case Page.NewVideo:
                    destination = "/View/NewVideo.xaml";
                    break;
                case Page.EditIUAN:
                    destination = "/View/EditAnalysis.xaml";
                    break;
                case Page.TaxonManagement:
                    destination = "/View/TaxonManagement.xaml";
                    break;
                case Page.Sync:
                    destination = "/View/Sync.xaml";
                    break;
                case Page.EditEventProperty:
                    destination = "/View/EditEventProperty.xaml";
                    break;
                case Page.Setup:
                    destination = "/View/Setup.xaml";
                    break;

#if DEBUG
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
#endif
            }                

            if (destination != null && _frame != null)
            {
                States[visitCounter.ToString()] = new PageState(msg.Destination, msg.Context, msg.ReferrerType, msg.Referrer);
                var destURI = new Uri(
                    string.Format("{0}?{1}={2}", destination, VISIT_KEY, visitCounter++),
                    UriKind.RelativeOrAbsolute);
                _frame.Dispatcher.BeginInvoke(() => _frame.Navigate(destURI));                               
            }
        }  

        public void NavigateBack()
        {           

            _frame.GoBack();
        }     
      
    }
}
