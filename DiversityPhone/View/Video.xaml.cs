using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using ReactiveUI.Xaml;
using DiversityPhone.View.Appbar;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows;
using System.Reactive.Disposables;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class NewVideo : PhoneApplicationPage
    {


        private IsolatedStorageFileStream isoVideoFile;
        private VideoBrush videoRecorderBrush;
        private CaptureSource captureSource;
        private VideoCaptureDevice videoCaptureDevice;
        private FileSink fileSink;

        private IDisposable _subscriptions;


        private VideoVM VM
        {
            get
            {
                return DataContext as VideoVM;
            }
        }

        private CommandButtonAdapter _record;
        private PlayStopButton _playstop;
        private SaveDeleteButton _savedelete;
        
         

        public NewVideo()
        {
            InitializeComponent();
           
            _record = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.Record);
            _playstop = new PlayStopButton(ApplicationBar, VM);
            _savedelete = new SaveDeleteButton(ApplicationBar, VM);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            initializeVideoRecorder();

            _subscriptions = new CompositeDisposable()
            {
                VM.Record.Subscribe(_ => record()),
                VM.Play.Subscribe(_ => play()),
                VM.Stop.Subscribe(_ => stop()),
                _record,
                _playstop,
                _savedelete
            };

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Dispose of camera and media objects.
            _subscriptions.Dispose();
            disposeVideoRecorder();
            disposeVideoPlayer();

            base.OnNavigatedFrom(e);
        }

        #region Methods for Reactive Commands

        private void record()
        {
            this.startVideoRecording();
        }

        private void play()
        {
            this.startPlayback();
        }

        private void stop()
        {
            if (VM.State == PlayStates.Recording)
                stopVideoRecording();
            else if (VM.State == PlayStates.Playing)
                this.stopPlayback();
        }

        #endregion


        #region Recording

        private void startVideoRecording()
        {
            try
            {
                // Connect fileSink to captureSource.
                if (captureSource.VideoCaptureDevice != null
                    && captureSource.State == CaptureState.Started)
                {
                    captureSource.Stop();
                    // Connect the input and output of fileSink.
                    fileSink.CaptureSource = captureSource;
                    fileSink.IsolatedStorageFileName = VM.TempFileName;
                }
                // Begin recording.
                if (captureSource.VideoCaptureDevice != null
                    && captureSource.State == CaptureState.Stopped)
                {
                    captureSource.Start();
                }
                VM.State = PlayStates.Recording;
            }

            // If recording fails, display an error.
            catch (Exception)
            {
                MessageBox.Show("Recording error");
            }
        }

        public void stopVideoRecording()
        {
            try
            {
                // Stop recording.
                if (captureSource.VideoCaptureDevice != null
                && captureSource.State == CaptureState.Started)
                {
                    captureSource.Stop();

                    // Disconnect fileSink.
                    fileSink.CaptureSource = null;
                    fileSink.IsolatedStorageFileName = null;
                    VM.RecordPresent = true;

                }
                VM.State = PlayStates.Idle;
            }
            catch (Exception)
            {
                MessageBox.Show("Recording error");
                VM.State = PlayStates.Idle;
            }
            startVideoPreview();
        }


        // Set the recording state: display the video on the viewfinder.
        public void startVideoPreview()
        {
            try
            {
                // Display the video on the viewfinder.
                if (captureSource.VideoCaptureDevice != null
                && captureSource.State == CaptureState.Stopped)
                {
                    videoRecorderBrush.SetSource(captureSource);
                    viewfinderRectangle.Fill = videoRecorderBrush;
                    captureSource.Start();
                }
            }
            catch (Exception )
            {
                MessageBox.Show("Preview Error");
            }
        }

        #endregion

        #region Playback

        private void startPlayback()
        {

            // Start video playback when the file stream exists.
            if (isoVideoFile != null)
            {
  
                videoPlayer.Play();
            }
            // Start the video for the first time.
            else
            {
                var filename = (VM.IsEditable) ? VM.TempFileName : VM.Current.Model.Uri;

                captureSource.Stop();
                // Remove VideoBrush from the tree.
                viewfinderRectangle.Fill = null;
                isoVideoFile = new IsolatedStorageFileStream(filename,
                                        FileMode.Open, FileAccess.Read,
                                        IsolatedStorageFile.GetUserStoreForApplication());

                videoPlayer.SetSource(isoVideoFile);
                videoPlayer.MediaEnded += new RoutedEventHandler(VideoPlayerMediaEnded);
                videoPlayer.Play();
            }
            VM.State = PlayStates.Playing;
        }

        private void stopPlayback()
        {
            disposeVideoPlayer();
            if(VM.IsEditable)
                startVideoPreview();
        }


        #endregion



        #region Events


        private void OnCaptureFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("No VideoRecording device found:" + e.ErrorException.Message);
        }

        // Display the viewfinder when playback ends.
        public void VideoPlayerMediaEnded(object sender, RoutedEventArgs e)
        {
            disposeVideoPlayer();
            if (VM.IsEditable)
                startVideoPreview();
        }

        #endregion

        #region Initialize

        public void initializeVideoRecorder()
        {
            if (captureSource == null)
            {
                captureSource = new CaptureSource();
                fileSink = new FileSink();
                videoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();

                // Add eventhandlers for captureSource.
                captureSource.CaptureFailed += new EventHandler<ExceptionRoutedEventArgs>(OnCaptureFailed);
                if (videoCaptureDevice != null)
                {
                    videoRecorderBrush = new VideoBrush();
                    videoRecorderBrush.SetSource(captureSource);
                    viewfinderRectangle.Fill = videoRecorderBrush;
                    captureSource.Start();
                }
            }
        }

        #endregion

        #region Dispose


        private void disposeVideoPlayer()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.Source = null;
                isoVideoFile = null;
                videoPlayer.MediaEnded -= VideoPlayerMediaEnded;
            }
            VM.State = PlayStates.Idle;
        }

        public void disposeVideoRecorder()
        {
            if (captureSource != null)
            {
                if (captureSource.VideoCaptureDevice != null
                    && captureSource.State == CaptureState.Started)
                {
                    captureSource.Stop();
                }
                captureSource.CaptureFailed -= OnCaptureFailed;
                captureSource = null;
                videoCaptureDevice = null;
                fileSink = null;
                videoRecorderBrush = null;
            }
        }

        #endregion

      

    }
}