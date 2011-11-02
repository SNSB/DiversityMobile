using System;
using System.Reactive.Linq;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class EditCSVM : ReactiveObject
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


        public EditCSVM(IMessageBus messenger)
        {

            _messenger = messenger;

            _Model = _messenger.Listen<Specimen>(MessageContracts.EDIT)
                .ToProperty(this, x => x.Model);

            var modelPropertyObservable = this.ObservableForProperty(x => x.Model)
                .Select(change => change.Value)
                .Where(x => x != null);

            var canSave = this.ObservableForProperty(x => x.AccessionNumber)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => enableEdit()),        
               
                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),
                //Read-Only Eigenschaften direkt ans Model Binden
                //Nur Veränderbare Properties oder abgeleitete so binden
                modelPropertyObservable                    
                    .Select(m => m.AccesionNumber != null ? m.AccesionNumber : "")
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


        private void enableEdit()
        {
        }

        private void updateModel()
        {
            //Nur Veränderbare Eigenschaften übernehmen.
            Model.AccesionNumber = AccessionNumber;
        }
    }
}
