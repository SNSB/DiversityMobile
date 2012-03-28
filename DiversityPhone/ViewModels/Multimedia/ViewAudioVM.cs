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
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;
namespace DiversityPhone.ViewModels
{
    public class ViewAudioVM : EditElementPageVMBase<MultimediaObject>
    {
    

        #region Properties
        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }
        #endregion


        public ViewAudioVM()            
        {


        }

        private void PlayAudio(MultimediaObject mmo)
        {

  
         
        }
      

        private void executeSave()
        {
            
        }



        protected override void OnDelete()
        {            
            Messenger.SendMessage<MultimediaObject>(Current.Model, MessageContracts.DELETE);
           
            Messenger.SendMessage<Message>(Message.NavigateBack);
        }

        protected override void UpdateModel()
        {
            
        }

      

        protected override MultimediaObject ModelFromState(Services.PageState s)
        {
            if (s.Context != null)
            {
                MultimediaObject mmo = Storage.getMultimediaByURI(s.Context);
                return mmo;
            }
            else if (s.Referrer != null)
            {
                int parent;
                if (int.TryParse(s.Referrer, out parent))
                {
                    return new MultimediaObject()
                        {
                            RelatedId = parent,
                            OwnerType = s.ReferrerType,
                        };
                }
            }

            return null;
        }

        public override void SaveState()
        {
            base.SaveState();

        }


        protected override IObservable<bool> CanSave()
        {
            return Observable.Return(false);
        }

        protected override ElementVMBase<MultimediaObject> ViewModelFromModel(MultimediaObject model)
        {
            return new MultimediaObjectVM(Messenger, model, DiversityPhone.Services.Page.Current);
        }
    }
}
