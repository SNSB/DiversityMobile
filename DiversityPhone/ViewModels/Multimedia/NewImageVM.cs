using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using Funq;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Devices;
using System.Windows;

namespace DiversityPhone.ViewModels
{
    public class NewImageVM : EditElementPageVMBase<MultimediaObject>
    {

        public PhotoCamera Camera { get; private set; }

        #region Properties

        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }


        private BitmapImage _oldImage;

        public BitmapImage OldImage
        {
            get
            {
                return _oldImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.OldImage, ref _oldImage, value);
            }
        }

        private BitmapImage _actualImage;

        public BitmapImage ActualImage
        {
            get
            {
                return _actualImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ActualImage, ref _actualImage, value);
            }
        }

        private bool _shootingEnabled = true;
        public bool ShootingEnabled //Converter for image and vidfeoBrush in View needed
        {
            get
            {
                return _shootingEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ShootingEnabled, ref _shootingEnabled, value);
                this.raisePropertyChanged("ShootingDisabled");
            }
        }
        public bool ShootingDisabled //For DataBinding
        {
            get
            {
                return !_shootingEnabled;
            }
        }

        #endregion 

        #region Commands

        public ReactiveCommand Reset { get; private set; }
        public ReactiveCommand Take { get; private set; } //Action depending on ShootingEnabled
        //public ReactiveCommand Crop { get; private set; }

        #endregion


        public NewImageVM()
        {
            Reset = new ReactiveCommand();
            Reset.Subscribe(_ => refresh());
            Take = new ReactiveCommand();
            Take.Subscribe(_ => take());
            this.initializeCamera();

        }

        private void initializeCamera()
        {
            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
                this.Camera = new PhotoCamera(CameraType.Primary);
            else
            {
                MessageBox.Show("Cannot find a camera on this device");
                return;
            }
            this.Camera.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(Camera_CaptureImageAvailable);
            this.Camera.Initialized += new EventHandler<CameraOperationCompletedEventArgs>(Camera_Initialized);
            this.Camera.AutoFocusCompleted += new EventHandler<CameraOperationCompletedEventArgs>(Camera_AutoFocusCompleted);
        }

        protected override void UpdateModel()
        {
            saveImage();
            Current.Model.LogUpdatedWhen = DateTime.Now;
            Current.Model.Uri = Uri;
        }


        protected override void OnSave()
        {
            
        }

        public override void SaveState()
        {
            base.SaveState();
        }

        protected override MultimediaObject ModelFromState(Services.PageState s)
        {
            if (s.Referrer != null)
            {
                int parent;
                if (int.TryParse(s.Referrer, out parent))
                {
                    return new MultimediaObject()
                    {
                        RelatedId = parent,
                        OwnerType = s.ReferrerType,
                        MediaType=MediaType.Image,
                    };
                }
            }

            return null;
        }

        protected override IObservable<bool> CanSave()
        {
            return this.ObservableForProperty(x => x.Uri)
                .Select(uri=> !string.IsNullOrEmpty(uri.Value))
                .StartWith(false);
        }

        protected override ElementVMBase<MultimediaObject> ViewModelFromModel(MultimediaObject model)
        {
            return new MultimediaObjectVM(model);
        }

        private void take()
        {
            if (ShootingEnabled)
            {
                this.Camera.CaptureImage();
            }
            else
                ShootingEnabled = true;
        }

        private void Camera_CaptureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                BitmapImage image = new BitmapImage();
                image.SetSource(e.ImageStream);
                this.OldImage = ActualImage;
                this.ActualImage = image;
                this.ShootingEnabled = false;
            });
        }

        private void refresh()
        {
            if(OldImage!=null)
                ActualImage = OldImage;
        }

        private void Camera_Initialized(object sender, CameraOperationCompletedEventArgs e)
        {

            if (this.Camera.IsFlashModeSupported(FlashMode.Auto))
                this.Camera.FlashMode = FlashMode.Auto;

        }

        private void Camera_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e) //Einbinden
        {
            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    this.cameraView.BorderBrush = new SolidColorBrush(Colors.Red);
            //});
        }

        private void saveImage()
        {
            //Save Picture
            //

            //Make progress bar visible for the event handler as there may be posible latency.
            //progressBar1.Visibility = Visibility.Visible; //Work with iBusy and AsyncCommands

            //Create filename for JPEG in isolated storage as a Guid and filename for thumb
            String uri;

            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                Guid g = Guid.NewGuid();
                uri = g.ToString() + ".jpg";
            }
            else
            {
                uri = Current.Model.Uri;
            }
            //Create virtual store and file stream. Check for duplicate tempJPEG files.
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(uri))
            {
                myStore.DeleteFile(uri);
            }
            IsolatedStorageFileStream myFileStream = myStore.CreateFile(uri);
            //Encode the WriteableBitmap into JPEG stream and place into isolated storage.
            System.Windows.Media.Imaging.Extensions.SaveJpeg(new WriteableBitmap(this.ActualImage), myFileStream, this.ActualImage.PixelWidth, this.ActualImage.PixelHeight, 0, 85);
            myFileStream.Close();
            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                this.Uri = uri;

            }
            //progressBar1.Visibility = Visibility.Collapsed;
        }

    }
}
