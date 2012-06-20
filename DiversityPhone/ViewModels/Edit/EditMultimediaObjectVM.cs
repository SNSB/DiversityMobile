using System;
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


        #region Properties
 

        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }
      

        private BitmapImage _savedImage;

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
        #endregion

        public EditMultimediaObjectVM()            
        {


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
         
        }
      

        private void executeSave()
        {
            UpdateModel();
            Messenger.SendMessage<MultimediaObject>(Current.Model, MessageContracts.SAVE);
        }



        protected override void OnDelete()
        {            
            Messenger.SendMessage<MultimediaObject>(Current.Model, MessageContracts.DELETE);
           
            Messenger.SendMessage(Page.Previous);
        }

        protected override void UpdateModel()
        {
            Current.Model.LogUpdatedWhen = DateTime.Now;
            Current.Model.Uri = Uri;
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

        public override void SaveState()
        {
            base.SaveState();
        }

        protected override IObservable<bool> CanSave()
        {
            return this.ObservableForProperty(x => x.SavedImage)
                .Select(im => im.Value.UriSource != null)
                .StartWith(false);
        }

 

        protected override ElementVMBase<MultimediaObject> ViewModelFromModel(MultimediaObject model)
        {
            return new MultimediaObjectVM(model);
        }
    }

}
