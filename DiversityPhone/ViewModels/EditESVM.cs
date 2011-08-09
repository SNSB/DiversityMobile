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

        private string _Description;

        public string Description
        {
            get { return _Description; }
            set { this.RaiseAndSetIfChanged(x=>x.Description,value); }
        }
        

        public EditESVM(INavigationService nav, IMessageBus messenger)
        {
            _navigation = nav;
            _messenger = messenger;

            var descriptionObservable = this.ObservableForProperty(x => x.Description);
            var canSave = descriptionObservable.Select(desc => !string.IsNullOrWhiteSpace(desc.Value));

            (Save = new ReactiveCommand(canSave))               
                .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model, MessageContracts.SAVE));
        }        
    }
}
