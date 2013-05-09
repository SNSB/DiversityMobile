using System;
using Microsoft.Phone.Controls;
using ReactiveUI;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using System.Reactive.Linq;
using System.Reactive.Concurrency;


namespace DiversityPhone.Services
{
    public class NavigationService
    {
        readonly IMessageBus Messenger;
        readonly PhoneApplicationFrame _frame;

        private PageVMBase _CurrentVM;
        public PageVMBase CurrentVM
        {
            get { return _CurrentVM; }
            set
            {
                if (value != null && _CurrentVM != value)
                {
                    if (_CurrentVM != null)
                        _CurrentVM.Deactivate();
                    _CurrentVM = value;
                    _CurrentVM.Activate();
                }

            }
        }


        public NavigationService(
            IMessageBus messenger,
            PhoneApplicationFrame RootFrame,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            Messenger = messenger;
            _frame = RootFrame;

            _frame.Navigated += (s, args) => { NavigationFinished(); };

            var mmo = Observable.Merge(
                Messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.EDIT)
                );

            Messenger.RegisterMessageSource(
                Observable.Merge(
                    Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.VIEW).Select(_ => Page.ViewES),
                    Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.EDIT).Select(_ => Page.EditES),
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
                    Messenger.Listen<ILocalizable>(MessageContracts.VIEW).Select(_ => Page.ViewMap),
                    Messenger.Listen<ILocationOwner>(MessageContracts.VIEW).Select(_ => Page.ViewMap)
                    )
                );

            Messenger.Listen<Page>()
               .ObserveOn(Dispatcher)
               .Subscribe(NavigateToPage);
        }

        void NavigationFinished()
        {
            var page = _frame.Content as PhoneApplicationPage;
            if (page != null)
                CurrentVM = page.DataContext as PageVMBase;
        }

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
                    destination = "/View/Image.xaml";
                    break;
                case Page.NewAudio:
                    destination = "/View/Audio.xaml";
                    break;
                case Page.NewVideo:
                    destination = "/View/Video.xaml";
                    break;
                case Page.EditIUAN:
                    destination = "/View/EditAnalysis.xaml";
                    break;
                case Page.TaxonManagement:
                    destination = "/View/TaxonManagement.xaml";
                    break;
                case Page.Upload:
                    destination = "/View/Upload.xaml";
                    break;
                case Page.Download:
                    destination = "/View/Download.xaml";
                    break;
                case Page.EditEventProperty:
                    destination = "/View/EditEventProperty.xaml";
                    break;
                case Page.Setup:
                    destination = "/View/Setup.xaml";
                    break;
                case Page.Info:
                    destination = "/View/Info.xaml";
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
                if (destURI != _frame.CurrentSource)
                {
                    _frame.Dispatcher.BeginInvoke(() => _frame.Navigate(destURI));
                }
            }
        }

        public void NavigateBack()
        {
            if (_frame.CanGoBack)
                _frame.GoBack();
        }

    }
}