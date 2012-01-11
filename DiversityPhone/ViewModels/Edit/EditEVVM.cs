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

        private ObservableAsPropertyHelper<string> _CollectionDate;
        public string CollectionDate
        {
            get
            {
                return  _CollectionDate.Value;
            }
        }
        #endregion

        public EditEVVM(IMessageBus messenger, IOfflineStorage storage)
            : base(messenger)
        {            
            
            _storage = storage;

            var model = StateObservable
                .Select(s => EventFromState(s));            

            _Model = model
                .ToProperty(this, vm => vm.Model);

            _CollectionDate = model
                .Select(ev => ev.CollectionDate.ToString())
                .ToProperty<EditEVVM,string>(this, vm => vm.CollectionDate);

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
            Messenger.SendMessage<Event>(Model, MessageContracts.SAVE);
            Messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void delete()
        {
            Messenger.SendMessage<Event>(Model, MessageContracts.DELETE);
            Messenger.SendMessage<Message>(Message.NavigateBack);
        }      


        private void updateModel()
        {
            Model.LocalityDescription = LocalityDescription;
            Model.HabitatDescription = HabitatDescription;            
        }

        private void updateView(Event ev)
        {            
            LocalityDescription = ev.LocalityDescription;
            HabitatDescription = ev.HabitatDescription;            
        }

        private Event EventFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return _storage.getEventByID(id);
                }
                else
                    return null;
            }
            else
            {
                var newEvent = new Event()
                {
                    SeriesID = null
                };

                int parent;
                if (s.Referrer != null && int.TryParse(s.Referrer, out parent))
                {
                    newEvent.SeriesID = parent;
                }
                return newEvent;
            }            
        }
    }
}
