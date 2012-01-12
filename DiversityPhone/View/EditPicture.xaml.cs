using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone;

using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using DiversityPhone.Model;

namespace DiversityPhone.View
{
    public partial class EditPicture : PhoneApplicationPage
    {

        //The camera chooser used to capture a picture.
        private CameraCaptureTask ctask;
        private MultimediaObject mmo;
        //Variable for the WriteableBitmap object
        public static WriteableBitmap capturedImage;

        public EditPicture()
        {
            InitializeComponent();
            SupportedOrientations = SupportedPageOrientation.Portrait | SupportedPageOrientation.Landscape;
            textStatus.Text = "Tap the camera button to take a picture.";

            //Create new instance of CameraCaptureClass
            ctask = new CameraCaptureTask();

            //Create new event handler for capturing a photo
            ctask.Completed += new EventHandler<PhotoResult>(ctask_Completed);

        }

        private void Camera_Click(object sender, EventArgs e)
        {
            textStatus.Text = "";

            //Show the camera.
            ctask.Show();

            //Set progress bar to visible to show time between user snapshot and decoding
            //of image into a writeable bitmap object.
            progressBar1.Visibility = Visibility.Visible;
        }

         void ctask_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {

                //Take JPEG stream and decode into a WriteableBitmap object
                capturedImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);

                //Collapse visibility on the progress bar once writeable bitmap is visible.
                progressBar1.Visibility = Visibility.Collapsed;


                //Populate image control with WriteableBitmap object.
                MainImage.Source = capturedImage;

                //Once writeable bitmap has been rendered, the crop button
                //is enabled.
                this.btn_Crop.IsEnabled=true;
               
                textStatus.Text = "Tap the crop button to proceed";
            }
            else
            {
                textStatus.Text = "You decided not to take a picture.";
            }
        }

        private void Crop_Click(object sender, EventArgs e)
        {
              //Error text for if user does not take a photo before choosing the crop button.
            if (capturedImage == null)
            {
                textStatus.Text = "You must take a picture first.";

            }
            else
            {
                //If photo has been taken, crop button navigates to Crop.xaml page.
                NavigationService.Navigate(new Uri("View/CropPicture.xaml", UriKind.Relative));
            }
        }
    }


    }
}