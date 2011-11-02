using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class EditEVVM : ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;        
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties
        private Event _Model;
        public Event Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value); }
        }

        private string _LocalityDescription;
        public string LocalityDescription
        {
            get { return _LocalityDescription; }
            set { this.RaiseAndSetIfChanged(x=>x.LocalityDescription, ref _LocalityDescription, value); }
        }

        private string _HabitatDescription;
        public string HabitatDescription
        {
            get { return _HabitatDescription; }
            set { this.RaiseAndSetIfChanged(x => x.HabitatDescription, ref _HabitatDescription, value); }
        }

        public string CollectionDate
        {
            get
            {
                return  "";
            }
        }
        #endregion

        public EditEVVM(IMessageBus messenger)
        {            
            _messenger = messenger;            
        
            var canSave = this.ObservableForProperty(x => x.LocalityDescription)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Message>(Message.NavigateBack)),

                _messenger.Listen<Event>(MessageContracts.EDIT)
                    .Subscribe(ev => updateView(ev))
            };
        }    

        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<Event>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void delete()
        {
            _messenger.SendMessage<Event>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void enableEdit()
        {
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
