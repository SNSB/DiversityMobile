using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using Funq;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using System.Windows;
using System.IO;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace DiversityPhone.ViewModels
{
    public class NewVideoVM : EditElementPageVMBase<MultimediaObject>
    {
        //VideoComponents

        // Viewfinder for capturing video.
        private VideoBrush videoRecorderBrush;
        private CaptureSource captureSource;   
        private IsolatedStorageFileStream isoVideoFile;
        private VideoCaptureDevice videoCaptureDevice;
        private FileSink fileSink;
        private string isoVideoFileName = "tempVideoSave.mp4";

        //Presentation Elements

        private MediaElement videoPlayer=new MediaElement();


        #region Properties

        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }


        private PlayStates _state = PlayStates.Idle; // Flag to monitor the state of sound playback and recording
        public PlayStates State
        {
            get
            {
                return _state;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.State, ref _state, value);
            }
        }

        private bool _recordPresent = false; //Flag to monitor if a record has been made
        public bool RecordPresent
        {
            get
            {
                return _recordPresent;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.RecordPresent, ref _recordPresent, value);
            }
        }

        private Brush _Fill = null;
        public Brush Fill
        {
            get
            {
                return _Fill;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Fill, ref _Fill, value);
            }
        }

        #endregion

        #region Commands


        public ReactiveCommand Record { get; private set; }
        public ReactiveCommand Play { get; private set; }
        public ReactiveCommand Stop { get; private set; }

        #endregion

        #region Constructor

        public NewVideoVM()
        {
            //In die View setzen und Appbar übergeben
            Record = new ReactiveCommand();
            Record.Subscribe(_ => record());
            Play = new ReactiveCommand();
            Play.Subscribe(_ => play());
            Stop = new ReactiveCommand();
            Stop.Subscribe(_ => stop());
            initializeVideoRecorder();
        }

        #endregion

        #region Inherited

        protected override void UpdateModel()
        {
            saveVideo();
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
                        MediaType = MediaType.Video
                    };
                }
            }
            return null;
        }

        protected override IObservable<bool> CanSave()
        {
            var idle = this.ObservableForProperty(x => x.State)
                .Select(sound => sound.Value == PlayStates.Idle)
                .StartWith(false);

            var recordPresent = this.ObservableForProperty(x => x.RecordPresent)
                .Select(present => present.Value)
                .StartWith(false);
            return idle.BooleanAnd(recordPresent);
        }


        protected override ElementVMBase<MultimediaObject> ViewModelFromModel(MultimediaObject model)
        {
            return new MultimediaObjectVM(model);
        }

        #endregion

        #region Methods for ReactiveCommands

        private void play()
        {
            this.startPlayback();
        }

        private void record()
        {
            this.startVideoRecording();
        }

        private void stop()
        {
            if (this.State == PlayStates.Recording)
                this.stopVideoRecording();
            else if (this.State == PlayStates.Playing)
                this.stopPlayback();
            else
                MessageBox.Show("Status error");
        }

        private void saveVideo()
        {
            String uri;
            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                Guid g = Guid.NewGuid();
                uri = g.ToString() + ".mp4";
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
            if (myStore.FileExists(isoVideoFileName))
            {
                myStore.MoveFile(isoVideoFileName, uri);
            }
            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                Uri = uri;
            }
        }
        #endregion

        #region Video

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
                    fileSink.IsolatedStorageFileName = isoVideoFileName;
                }
                // Begin recording.
                if (captureSource.VideoCaptureDevice != null
                    && captureSource.State == CaptureState.Stopped)
                {
                    captureSource.Start();
                }
                this.State = PlayStates.Recording;
            }

            // If recording fails, display an error.
            catch (Exception e)
            {
                MessageBox.Show("Recording error");
            }
        }

        private void stopVideoRecording()
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
                    RecordPresent = true;

                }
                this.State = PlayStates.Idle;
            }
            catch (Exception e)
            {
                MessageBox.Show("Recording error");
                this.State = PlayStates.Idle;
            }
            startVideoPreview();
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
                captureSource.Stop();

                // Remove VideoBrush from the tree.
                Fill = null;
                isoVideoFile = new IsolatedStorageFileStream(isoVideoFileName,
                                        FileMode.Open, FileAccess.Read,
                                        IsolatedStorageFile.GetUserStoreForApplication());

                videoPlayer.SetSource(isoVideoFile);
                videoPlayer.MediaEnded += new RoutedEventHandler(VideoPlayerMediaEnded);
                videoPlayer.Play();
            }
            videoRecorderBrush.SetSource(videoPlayer);
            Fill = videoRecorderBrush;
            this.State = PlayStates.Playing;
        }

        private void stopPlayback()
        {
            disposeVideoPlayer();
            startVideoPreview();
        }

        #endregion

        // Set the recording state: display the video on the viewfinder.
        private void startVideoPreview()
        {
            try
            {
                // Display the video on the viewfinder.
                if (captureSource.VideoCaptureDevice != null
                && captureSource.State == CaptureState.Stopped)
                {
                    videoRecorderBrush.SetSource(captureSource);
                    Fill = videoRecorderBrush;
                    captureSource.Start();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Preview Error");
            }
        }

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
                    Fill = videoRecorderBrush;
                    captureSource.Start();
                }
            }
        }


        #endregion

        #region Events


        private void OnCaptureFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("No VideoRecording device foiund:"+ e.ErrorException.Message);
        }

        // Display the viewfinder when playback ends.
        public void VideoPlayerMediaEnded(object sender, RoutedEventArgs e)
        {
            disposeVideoPlayer();
            startVideoPreview();
        }


        #endregion

        #region Dispose
        private void disposeVideoPlayer()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.Source = null;
                videoPlayer.MediaEnded -= VideoPlayerMediaEnded;
            }
            this.State = PlayStates.Idle;
        }

        private void disposeVideoRecorder()
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

        public void clean()
        {
            disposeVideoPlayer();
            disposeVideoRecorder();
        }
        #endregion
    }
}
