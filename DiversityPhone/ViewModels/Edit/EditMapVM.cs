using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class EditMapVM : ReactiveObject
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
        //Anpassen das editierbare Objekt sind die Geofelder eines Events

        public bool _editable;
        public bool Editable { get { return _editable; } set { this.RaiseAndSetIfChanged(x => x.Editable,ref _editable, value); } }


        private Map _Model;
        public Map Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value); }
        }


        #endregion

        public EditMapVM(IMessageBus messenger)
        {
            this._editable = false;
            _messenger = messenger;

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand())               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => setEdit()),

                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),

                _messenger.Listen<Map>(MessageContracts.EDIT)
                    .Subscribe(cep => updateView(cep))
            };
        }



        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<Map>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void setEdit()
        {
            if (Editable == false)
                Editable = true;
            else
                Editable = false;
        }


        private void delete()
        {
            _messenger.SendMessage<Map>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void updateModel()
        {

        }

        private void updateView(Map map)
        {
            this.Model = map;
        }
    }
}
