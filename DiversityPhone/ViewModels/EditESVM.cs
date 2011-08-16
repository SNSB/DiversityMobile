using System;
using System.Net;
using System.Reactive.Linq;
using ReactiveUI;
using DiversityService.Model;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class EditESVM : ReactiveObject
    {
        private IMessageBus _messenger;
        private INavigationService _navigation;

        public EventSeries Model { get; private set; }

        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Cancel { get; private set; }

        public string _Description; //Need to be public in SL :/

        public string Description
        {
            get { return _Description; }
            set { this.RaiseAndSetIfChanged(x=>x.Description,value); }
        }
        

        public EditESVM(INavigationService nav, IMessageBus messenger)
        {
            _navigation = nav;
            _messenger = messenger;

            _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                .Subscribe(es => editES(es));

            var descriptionObservable = this.ObservableForProperty(x => x.Description);
            var canSave = descriptionObservable.Select(desc => !string.IsNullOrWhiteSpace(desc.Value));

            (Save = new ReactiveCommand(canSave))               
                .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model, MessageContracts.SAVE));

            (Cancel = new ReactiveCommand())
                .Subscribe(_ => _navigation.NavigateBack());
        }

        private void editES(EventSeries es)
        {
            Model = es;
            Description = es.Description;
        }
    }
}
