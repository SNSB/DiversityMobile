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
    public class EditUserProfileVM:ReactiveObject
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

        public bool _editable;
        public bool Editable { get { return _editable; } set { this.RaiseAndSetIfChanged(x => x.Editable,ref _editable, value); } }


        private UserProfile _Model;
        public UserProfile Model
        {
            get { return _Model; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value);
            }
        }

        public string _LoginName;
        public string LoginName
        {
            get { return _AgentName; }
        }

        public string _AgentName;
        public string AgentName
        {
            get { return _AgentName; }
        } 

        private bool _recordGeoPosition = false;
        public bool RecordGeoPosition
        {
            get { return _recordGeoPosition; }
            set { this.RaiseAndSetIfChanged(x => x.RecordGeoPosition, ref _recordGeoPosition, value); }
        }

        #endregion

         public EditUserProfileVM(IMessageBus messenger)
        {

            _messenger = messenger;
            this._editable = false;

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand())               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => setEdit()),

                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),

               
            };
        }

         private void executeSave()
         {
             updateModel();
             _messenger.SendMessage<UserProfile>(Model, MessageContracts.SAVE);
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
             _messenger.SendMessage<UserProfile>(Model, MessageContracts.DELETE);
             _messenger.SendMessage<Message>(Message.NavigateBack);
         }

         private void updateModel()
         {
             Model.RecordGeoPosition = this.RecordGeoPosition;
         }

         private void updateView(UserProfile prof)
         {
             this.Model = prof;
             this.RecordGeoPosition = prof.RecordGeoPosition;
             this._LoginName = prof.LoginName;
             this._AgentName = prof.AgentName;

         }

    }
}
