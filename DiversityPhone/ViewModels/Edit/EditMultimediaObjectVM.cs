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
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class EditMultimediaObjectVM : ElementPageViewModel<MultimediaObject>
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties
        //Noch nicht fertig. Typ des MMO wählbar machen und Dialoge zur Aufnahme bereit stellen.
        public bool _editable;
        public bool Editable { get { return _editable; } set { this.RaiseAndSetIfChanged(x => x.Editable,ref _editable, value); } }       


        private ObservableAsPropertyHelper<MultimediaObject> _Model;
        public MultimediaObject Model
        {
            get { return _Model.Value; }            
        }


        #endregion

        public EditMultimediaObjectVM(IMessageBus messenger, IOfflineStorage storage)
            :base(messenger)
        {
            _storage = storage;
            
            this._editable = false;

            _Model = ValidModel
                .ToProperty(this, vm => vm.Model);

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
            Messenger.SendMessage<MultimediaObject>(Model, MessageContracts.SAVE);
            Messenger.SendMessage<Message>(Message.NavigateBack);
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
            Messenger.SendMessage<MultimediaObject>(Model, MessageContracts.DELETE);
            Messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void updateModel()
        {

        }       

        protected override MultimediaObject ModelFromState(Services.PageState s)
        {
            if (s.Referrer != null)
            {
                int parent;
                if (int.TryParse(s.Referrer, out parent))
                {
                    if (s.Context != null)
                        return _storage.getMultimediaByURI(s.Context);
                    else
                        return new MultimediaObject()
                         {
                             RelatedId = parent,
                             OwnerType = s.ReferrerType,
                             
                         };


                }
            }
            return null;
        }
    }
    
}
