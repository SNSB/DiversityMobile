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
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class EditMultimediaObjectVM:ReactiveObject
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
        //Noch nicht fertig. Typ des MMO wählbar machen und Dialoge zur Aufnahme bereit stellen.
        private MultimediaObject _Model;
        public MultimediaObject Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value); }
        }


        #endregion

        public EditMultimediaObjectVM(IMessageBus messenger)
        {

            _messenger = messenger;

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand())               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => enableEdit()),

                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),

                _messenger.Listen<MultimediaObject>(MessageContracts.EDIT)
                    .Subscribe(cep => updateView(cep))
            };
        }



        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<MultimediaObject>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }


        private void enableEdit()
        {
        }

        private void delete()
        {
            _messenger.SendMessage<MultimediaObject>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void updateModel()
        {

        }

        private void updateView(MultimediaObject mmo)
        {
            this.Model = mmo;
        }
    }
    
}
