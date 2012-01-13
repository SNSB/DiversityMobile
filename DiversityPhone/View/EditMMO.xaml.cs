using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;
using DiversityPhone.Services;
using Microsoft.Phone;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;


namespace DiversityPhone.View
{
    public partial class EditMMO : PhoneApplicationPage
    {
        private EditMultimediaObjectVM VM { get { return DataContext as EditMultimediaObjectVM; } }
        private WriteableBitmap capturedImage;
        private CameraCaptureTask takePhoto;
        

        public EditMMO()
        {
            InitializeComponent();
            //Create new instance of CameraCaptureClass
            takePhoto = new CameraCaptureTask();

            //Create new event handler for capturing a photo
            takePhoto.Completed += new EventHandler<PhotoResult>(takePhoto_Completed);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void NewPhoto_Click(object sender, EventArgs e)
        {
            VM.Model.MediaType = MediaType.Image;
           
            
            
            textStatus.Text = "";

            //Show the camera.
            takePhoto.Show();
            int i = takePhoto.GetHashCode();
            
        }

        private void NewAudio_Click(object sender, EventArgs e)
        {
            VM.Model.MediaType = MediaType.Audio;
        }

        private void NewVideo_Click(object sender, EventArgs e)
        {
            VM.Model.MediaType = MediaType.Video;
        }


        void takePhoto_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {
                //Set progress bar to visible to show time between user snapshot and decoding
                //of image into a writeable bitmap object.
                //progressBar1.Visibility = Visibility.Visible;

                //Take JPEG stream and decode into a WriteableBitmap object
                this.capturedImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);

                //Collapse visibility on the progress bar once writeable bitmap is visible.
                //progressBar1.Visibility = Visibility.Collapsed;


                //Populate image control with WriteableBitmap object.
                MainImage.Source = this.capturedImage;
                MainImage.Visibility = Visibility.Visible;

                //Adjust ApplicationBar
                this.ApplicationBar.IsVisible = false;

                //Save Picture
                //

                //Make progress bar visible for the event handler as there may be posible latency.
                progressBar1.Visibility = Visibility.Visible;

                //Create filename for JPEG in isolated storage as a Guid
                Guid g = Guid.NewGuid();
                String uri = g.ToString()+".jpg";

                //Create virtual store and file stream. Check for duplicate tempJPEG files.
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(uri))
                {
                    myStore.DeleteFile(uri);
                }
                IsolatedStorageFileStream myFileStream = myStore.CreateFile(uri);

                //Encode the WriteableBitmap into JPEG stream and place into isolated storage.
                System.Windows.Media.Imaging.Extensions.SaveJpeg(this.capturedImage, myFileStream, this.capturedImage.PixelWidth, this.capturedImage.PixelHeight, 0, 85);
                VM.Model.Uri = uri;
                myFileStream.Close();

            }
            else
            {
                textStatus.Text = "You decided not to take a picture.";
            }
        }



    }
}