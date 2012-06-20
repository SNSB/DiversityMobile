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
            _appbar = new NewImageAppBarUpdater(this.ApplicationBar, VM);

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e) 
        {
            this.cameraViewBrush.SetSource(VM.Camera);
            this.CorrectViewFinderOrientation(this.Orientation);
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(NewImagePage_OrientationChanged);
        }

       


        #region Orientation
        private void NewImagePage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            this.CorrectViewFinderOrientation(e.Orientation);
        }

        private void CorrectViewFinderOrientation(PageOrientation orientation)
        {
            if (orientation == PageOrientation.LandscapeLeft)
                this.cameraViewBrushTransform.Rotation = VM.Camera.Orientation - 90.0;
            else if (orientation == PageOrientation.LandscapeRight)
                this.cameraViewBrushTransform.Rotation = VM.Camera.Orientation + 90.0;
        }
        #endregion

       

    }
}