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
    public class EditEVVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        private IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties

        private ObservableAsPropertyHelper<bool> _isEditable;
        public bool Editable
        {
            get
            {
                return _isEditable.Value;
            }
        }


        private ObservableAsPropertyHelper<Event> _Model;
        public Event Model
        {
            get { return _Model.Value; }           
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

            var model = StateObservable
                .Select(s => EventFromContext(s.Context));            

            _Model = model
                .ToProperty(this, vm => vm.Model);

            ToggleEditable = new ReactiveCommand();

            _isEditable = StateObservable
               .Select(s => s.Context == null) //Newly created Units are immediately editable
               .Merge(
                   ToggleEditable.Select(_ => !Editable) //Toggle Editable
               )
               .ToProperty(this, vm => vm.Editable);

            var canSave = this.ObservableForProperty(x => x.LocalityDescription)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),         

                model.Subscribe( m => updateView(m)),
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


        private void updateModel()
        {
            Model.LocalityDescription = LocalityDescription;
            Model.HabitatDescription = HabitatDescription;            
        }

        private void updateView(Event ev)
        {            
            LocalityDescription = Model.LocalityDescription;
            HabitatDescription = Model.HabitatDescription;
            this.RaisePropertyChanged(x => x.CollectionDate);
        }

        private Event EventFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getEventByID(id);
                }
            }
            return new IdentificationUnit();
        }
    }
}
