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

        private IMessageBus Messenger;        
        private PhoneApplicationFrame _frame;

        //LEGACY
        private Stack<PageState> _legacy_navigation = new Stack<PageState>();
        //LEGACY

        public NavigationService(IMessageBus messenger)
        {
            Messenger = messenger;

            var mmo = Observable.Merge(
                Messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.EDIT),
                Messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.VIEW)
                );
            
            Messenger.RegisterMessageSource(
                Observable.Merge(
                    Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.VIEW).Select(_ => Page.ViewES),
                    Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.EDIT).Select(_ => Page.EditES),
                    Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.MAPS).Select(_ => Page.ViewMap),
                    Messenger.Listen<IElementVM<Event>>(MessageContracts.VIEW).Select(_ => Page.ViewEV),
                    Messenger.Listen<IElementVM<Event>>(MessageContracts.EDIT).Select(_ => Page.EditEV),
                    Messenger.Listen<IElementVM<Specimen>>(MessageContracts.VIEW).Select(_ => Page.ViewCS),
                    Messenger.Listen<IElementVM<Specimen>>(MessageContracts.EDIT).Select(_ => Page.EditCS),
                    Messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.VIEW).Select(_ => Page.ViewIU),
                    Messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.EDIT).Select(_ => Page.EditIU),                    
                    Messenger.Listen<IElementVM<EventProperty>>(MessageContracts.EDIT).Select(_ => Page.EditEventProperty),                   
                    Messenger.Listen<IElementVM<IdentificationUnitAnalysis>>(MessageContracts.EDIT).Select(_ => Page.EditIUAN),
                    mmo.Where(vm => vm.Model.MediaType == MediaType.Image).Select(_ => Page.NewImage),
                    mmo.Where(vm => vm.Model.MediaType == MediaType.Video).Select(_ => Page.NewVideo),
                    mmo.Where(vm => vm.Model.MediaType == MediaType.Audio).Select(_ => Page.NewAudio),
                    Messenger.Listen<ILocalizable>(MessageContracts.VIEW).Select(_ => Page.ViewMap)
                    )
                );

             Messenger.Listen<Page>()                
                .Subscribe(NavigateToPage);

             Messenger.Listen<NavigationMessage>()
                 .Subscribe(NavigationMessageNavigation);
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
            
            if (page.DataContext is PageVMBase)
            {
                var vm = page.DataContext as PageVMBase;
                vm.Activate();
            }

            
            if (page.DataContext is PageViewModel && _legacy_navigation.Any())
            {
                (page.DataContext as PageViewModel).SetState(_legacy_navigation.Peek());
            }
        }        
        void NavigationStarted()
        {
            var page = _frame.Content as PhoneApplicationPage;

            if (page != null && page.DataContext is PageVMBase)
            {
                var vm = page.DataContext as PageVMBase;               
                vm.Deactivate();
            }           
        }

        //LEGACY///////////////////////////////////////////
        void NavigationMessageNavigation(NavigationMessage msg)
        {
            //Real Nav ?
            if (msg.Destination != Page.Previous && msg.Destination != Page.Current)
            {
                _legacy_navigation.Push(new PageState(
                        msg.Destination,
                        msg.Context,
                        msg.ReferrerType,
                        msg.Referrer));                
            }
            else if (msg.Destination == Page.Previous)
            {
                _legacy_navigation.Pop();
            }

            NavigateToPage(msg.Destination);
        }        
        //LEGACY///////////////////////////////////////////


        private void NavigateToPage(Page p)
        {
            string destination = null;
            switch (p)
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
                case Page.MapManagement:
                    destination = "/View/MapManagement.xaml";
                    break;
                case Page.ViewMap:
                    destination = "/View/ViewMap.xaml";
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
                var destURI = new Uri(destination, UriKind.RelativeOrAbsolute);
                if(destURI != _frame.CurrentSource)
                    _frame.Dispatcher.BeginInvoke(() => _frame.Navigate(destURI));
            }
        }       

        public void NavigateBack()
        {
            _frame.GoBack();
        }     
      
    }
}
