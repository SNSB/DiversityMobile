using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;
using System.Windows;
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;

namespace DiversityPhone.View
{
    public partial class NewImage : PhoneApplicationPage
    {

        private PhotoCamera Camera { get; set; }


        private NewImageVM VM
        {
            get
            {
                return DataContext as NewImageVM;
            }
        }
        private NewImageAppBarUpdater _appbar;

        public NewImage()
        {
            InitializeComponent();
            VM.Reset.Subscribe(_ => refresh());
            VM.Take.Subscribe(_ => take());
            _appbar = new NewImageAppBarUpdater(this.ApplicationBar, VM);
            this.initializeCamera();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e) 
        {
            this.cameraViewBrush.SetSource(this.Camera);
            this.CorrectViewFinderOrientation(this.Orientation);
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(NewImagePage_OrientationChanged);
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

        private void take()
        {
            if (VM.ShootingEnabled)
            {
                this.Camera.CaptureImage();
            }
            else
                VM.ShootingEnabled = true;
        }

        private void Camera_CaptureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                BitmapImage image = new BitmapImage();
                image.SetSource(e.ImageStream);
                VM.OldImage = VM.ActualImage;
                VM.ActualImage = image;
                VM.ShootingEnabled = false;
            });
        }

        private void refresh()
        {
            if (VM.OldImage != null)
                VM.ActualImage = VM.OldImage;
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

        #region Orientation
        private void NewImagePage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            this.CorrectViewFinderOrientation(e.Orientation);
        }

        private void CorrectViewFinderOrientation(PageOrientation orientation)
        {
            if (orientation == PageOrientation.LandscapeLeft)
                this.cameraViewBrushTransform.Rotation = this.Camera.Orientation - 90.0;
            else if (orientation == PageOrientation.LandscapeRight)
                this.cameraViewBrushTransform.Rotation = this.Camera.Orientation + 90.0;
        }
        #endregion

       

    }
}