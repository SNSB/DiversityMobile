using System;
using System.Reactive.Linq;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class EditCSVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        private IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties

        public bool _editable;
        public bool Editable { get { return _editable; } set { this.RaiseAndSetIfChanged(x => x.Editable, ref _editable,value); } }

        private ObservableAsPropertyHelper<Specimen> _Model;
        public Specimen Model
        {
            get { return _Model.Value; }
        }

        private string _AccessionNumber;
        public string AccessionNumber
        {            
            get { return _AccessionNumber; }
            set { this.RaiseAndSetIfChanged(x => x.AccessionNumber, ref _AccessionNumber, value); }
        }
        #endregion


        public EditCSVM(IMessageBus messenger, IOfflineStorage storage)
        {

            _messenger = messenger;
            _storage = storage;

            _Model = StateObservable
                .Select(s => SpecimenFromContext(s.Context))
                .ToProperty(this, x => x.Model);
            this._editable = false;
            var modelPropertyObservable = this.ObservableForProperty(x => x.Model)
                .Select(change => change.Value)
                .Where(x => x != null);

            var canSave = this.canSave();

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => setEdit()),        
               
                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),
                //Read-Only Eigenschaften direkt ans Model Binden
                //Nur Veränderbare Properties oder abgeleitete so binden
                modelPropertyObservable                    
                    .Select(m => m.AccessionNumber != null ? m.AccessionNumber : "")
                    .BindTo(this, x=>x.AccessionNumber),      
                  
            };
        }

        IObservable<bool> canSave()
        {
            IObservable<bool> accessionNumber=this.ObservableForProperty(x => x.AccessionNumber)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);
            IObservable<bool> canSave = accessionNumber;
            return canSave;
        }

        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<Specimen>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void delete()
        {
            _messenger.SendMessage<Specimen>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }


        private void setEdit()
        {
            if (Editable == false)
                Editable = true;
            else
                Editable = false;
        }


        private void updateModel()
        {
            //Nur Veränderbare Eigenschaften übernehmen.
            Model.AccessionNumber = AccessionNumber;
        }

        private Specimen SpecimenFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getSpecimenByID(id); 
                }
            }
            return new Specimen();
        }
    }
}
