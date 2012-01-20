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
    public class EditMultimediaObjectVM : EditElementPageVMBase<MultimediaObject>
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

        private BitmapImage _savedImage;
        public BitmapImage SavedImage { get { return _savedImage; } set { this.RaiseAndSetIfChanged(x => x.SavedImage, ref _savedImage, value); } }

        private ObservableAsPropertyHelper<BitmapImage> _bi; //Wie Wert übergeben?
        public BitmapImage SavedImage
        {
            get
            {
                return _savedImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SavedImage, ref _savedImage, value);
            }
        }

            this._editable = false;


        #endregion

        public EditMultimediaObjectVM()            
        {
                (Save = new ReactiveCommand())               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => setEdit()),

                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),
            };

 
        
        }

        private void LoadImage(MultimediaObject mmo)
        {

            if (mmo.MediaType != MediaType.Image)
                return;
            // The image will be read from isolated storage into the following byte array

            byte[] data;
            // Read the entire image in one go into a byte array
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                // Open the file - error handling omitted for brevity

                // Note: If the image does not exist in isolated storage the following exception will be generated:

                // System.IO.IsolatedStorage.IsolatedStorageException was unhandled

                // Message=Operation not permitted on IsolatedStorageFileStream

                using (IsolatedStorageFileStream isfs = isf.OpenFile(mmo.Uri, FileMode.Open, FileAccess.Read))
                {

                    // Allocate an array large enough for the entire file

                    data = new byte[isfs.Length];



                    // Read the entire file and then close it

                    isfs.Read(data, 0, data.Length);

                    isfs.Close();

                }

            }



            // Create memory stream and bitmap

            MemoryStream ms = new MemoryStream(data);
            BitmapImage bi = new BitmapImage();
            bi.SetSource(ms);
            SavedImage = bi;
            //Observable -- kann ich irgendwie das Bild auuch in _bi speichern?

        }
      

        private void executeSave()
        {
            updateModel();
            Messenger.SendMessage<MultimediaObject>(Model, MessageContracts.SAVE);
        }


        private void setEdit()
        {
            if (Editable == false)
                Editable = true;
            else
                Editable = false;
        }


        protected override void OnDelete()
        {            
            Messenger.SendMessage<MultimediaObject>(Model, MessageContracts.DELETE);
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(Current.Model.Uri))
            {
                myStore.DeleteFile(Current.Model.Uri);
            }
            Messenger.SendMessage<Message>(Message.NavigateBack);
        }

        protected override void UpdateModel()
        {
            Current.Model.LogUpdatedWhen = DateTime.Now;
        }

        protected override MultimediaObject ModelFromState(Services.PageState s)
        {
            if (s.Context != null)
            {
                MultimediaObject mmo = Storage.getMultimediaByURI(s.Context);
                if (mmo != null && mmo.MediaType == MediaType.Image)
                    LoadImage(mmo);
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

        protected override IObservable<bool> CanSave()
        {
            return Observable.Return(false);
        }

        protected override ElementVMBase<MultimediaObject> ViewModelFromModel(MultimediaObject model)
        {
            return new MultimediaObjectVM(Messenger, model, Page.Current);
        }
    }

}
