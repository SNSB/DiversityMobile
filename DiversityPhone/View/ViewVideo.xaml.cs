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
using DiversityPhone.ViewModels;
using System.Windows.Data;
using System.IO.IsolatedStorage;
using System.IO;

namespace DiversityPhone.View
{
    public partial class ViewVideo : PhoneApplicationPage
    {

        private ViewVideoVM VM { get { return this.DataContext as ViewVideoVM; } }
        private IsolatedStorageFileStream isoVideoFile;
      
        private ViewAudioVideoPageAppbarUpdater _appb;

        public ViewVideo()
        {
            InitializeComponent();
            VM.Play.Subscribe(_ => this.playVideo());
            VM.Stop.Subscribe(_ => this.stopPlaying());
            _appb = new ViewAudioVideoPageAppbarUpdater(this.ApplicationBar, VM);
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            DisposeVideoPlayer();
            base.OnNavigatedFrom(e);
        }

        private void playVideo()
        {

            imageStart.Visibility = Visibility.Collapsed;
            Video.Visibility = Visibility.Visible;
            // Start video playback when the file stream exists.
            if (isoVideoFile != null)
            {
                Video.Play();
            }
            // Start the video for the first time.
            else
            {
                // Create the file stream and attach it to the MediaElement.
                isoVideoFile = new IsolatedStorageFileStream(VM.Uri,
                                        FileMode.Open, FileAccess.Read,
                                        IsolatedStorageFile.GetUserStoreForApplication());

                Video.SetSource(isoVideoFile);

                // Add an event handler for the end of playback.
                Video.MediaEnded += new RoutedEventHandler(VideoPlayerMediaEnded);

                // Start video playback.
                Video.Play();
            }

        }

        private void stopPlaying()
        {
            Video.Stop();
        }

        public void DisposeVideoPlayer()
        {
            if (Video != null)
            {
                // Stop the VideoPlayer MediaElement.
                Video.Stop();

                // Remove playback objects.
                Video.Source = null;

                // Remove the event handler.
                Video.MediaEnded -= VideoPlayerMediaEnded;
            }
        }


        // Display the viewfinder when playback ends.
        public void VideoPlayerMediaEnded(object sender, RoutedEventArgs e)
        {
            _appb.adjustPlaying(false);
            // Remove the playback objects.
            //DisposeVideoPlayer();

        }

    }
}