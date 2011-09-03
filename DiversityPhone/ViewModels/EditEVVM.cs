using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class EditEVVM : ReactiveObject
    {
        private IMessageBus _messenger;
        private IOfflineStorage _storage;

        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Cancel { get; private set; }
        

        public Event _Model; //Need to be public in SL :/

        public Event Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, value); }
        }

        public string _LocalityDescription; //Need to be public in SL :/

        public string LocalityDescription
        {
            get { return _LocalityDescription; }
            set { this.RaiseAndSetIfChanged(x=>x.LocalityDescription,value); }
        }

        public string _HabitatDescription; //Need to be public in SL :/

        public string HabitatDescription
        {
            get { return _HabitatDescription; }
            set { this.RaiseAndSetIfChanged(x => x.HabitatDescription, value); }
        }

        public string CollectionDate
        {
            get
            {
                return (Model != null) ? Model.CollectionDate.ToLongDateString() : "";
            }
        }
        

        public EditEVVM(IMessageBus messenger, IOfflineStorage storage)
        {            
            _messenger = messenger;
            _storage = storage;

            _messenger.Listen<Event>(MessageContracts.EDIT)
                .Subscribe(ev => updateView(ev));

            var descriptionObservable = this.ObservableForProperty(x => x.LocalityDescription);
            var canSave = descriptionObservable.Select(desc => !string.IsNullOrWhiteSpace(desc.Value)).StartWith(false);

            (Save = new ReactiveCommand(canSave))               
                .Subscribe(_ => executeSave());

            (Cancel = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<Message>(Message.NavigateBack));

        }

       

        private void executeSave()
        {
            updateModel();
            _storage.addEvent(Model);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void updateModel()
        {
            Model.LocalityDescription = LocalityDescription;
            Model.HabitatDescription = HabitatDescription;
            
        }

        private void updateView(Event ev)
        {
            Model = ev;
            LocalityDescription = Model.LocalityDescription;
            HabitatDescription = Model.HabitatDescription;
            this.RaisePropertyChanged(x => x.CollectionDate);
        }
    }
}
