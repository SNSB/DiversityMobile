using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
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
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Windows.Threading;
using System.Threading;


namespace DiversityPhone.View
{
    public partial class EditMMO : PhoneApplicationPage
    {
        private EditMultimediaObjectVM VM { get { return DataContext as EditMultimediaObjectVM; } }

        //Photo Components
        private WriteableBitmap storedImage=null; //Image as saved in the Isolated storage
        private WriteableBitmap actualImage=null; //Image saved after a cropping procedure
        private CameraCaptureTask takePhoto;
        //Variables for the crop feature
        System.Windows.Point p1, p2;
        bool cropping = false;

        //Audio Components
        private Microphone microphone = Microphone.Default;
        private DispatcherTimer timer;
        private byte[] audioBuffer;                             // Dynamic buffer to retrieve audio data from the microphone
        private MemoryStream audioStream = new MemoryStream();  // Stores the audio data for later playback
        private SoundEffectInstance soundInstance;              // Used to play back audio
        private bool soundIsPlaying = false;                    // Flag to monitor the state of sound playback
        private BitmapImage blankImage;
        private BitmapImage microphoneImage;
        private BitmapImage speakerImage;
        

        //VideoComponents
        private VideoBrush videoRecorderBrush;
        private MediaElement videoPlayer;
        private CaptureSource captureSource;
        // File details for storing the recording.        
        private IsolatedStorageFileStream isoVideoFile;
        private VideoCaptureDevice videoCaptureDevice;
        private string isoVideoFileName = "tempVideoSave.mp4";
        private FileSink fileSink;
        private bool isVideoRecording = false;

        //ApplicationBarAdjustment
        ApplicationBarIconButton btnRecord;
        ApplicationBarIconButton btnPlay;
        ApplicationBarIconButton btnStop;
  

        ApplicationBarIconButton btnCamera;
        ApplicationBarIconButton btnCrop;
        
        ApplicationBarIconButton btnReset;
        ApplicationBarIconButton btnSave;


        public EditMMO()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Dispose of camera and media objects.
            DisposeVideoPlayer();
            DisposeVideoRecorder();
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(isoVideoFileName))
            {
                myStore.DeleteFile(isoVideoFileName);
            }
            base.OnNavigatedFrom(e);
        }

      
        #region Photo

        private void btnNewPhoto_Click(object sender, EventArgs e)
        {
            //Create new instance of CameraCaptureClass
            takePhoto = new CameraCaptureTask();

            //Create new event handler for capturing a photo
            takePhoto.Completed += new EventHandler<PhotoResult>(takePhoto_Completed);

            //Used for rendering the cropping rectangle on the image.
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            this.PageTitle.Text = "photo";
            VM.Current.Model.MediaType = MediaType.Image;
            PhotoImage.Visibility = Visibility.Visible;


            textStatus.Text = "";
            setPhotoBar();
            //Show the camera.
            takePhoto.Show();

        }

        private void takePhoto_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {
                if (VM == null)
                    return;
                //Set progress bar to visible to show time between user snapshot and decoding
                //of image into a writeable bitmap object.
                progressBar1.Visibility = Visibility.Visible;

                //Take JPEG stream and decode into a WriteableBitmap object
                this.storedImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);
                this.actualImage = storedImage;
                PhotoImage.Source = actualImage;
                //Collapse visibility on the progress bar once writeable bitmap is visible.
                progressBar1.Visibility = Visibility.Collapsed;
                
                setPhotoButtonStates(true, true, false, true);
                
            }
            else
            {
                textStatus.Text = "You decided not to take a picture.";
                setPhotoBar();
                setPhotoButtonStates(true, false, false, false);
            }
            
        }

        #region ImageButtons

        private void btnCaptureImage_Click(object sender, EventArgs e)
        {
            takePhoto.Show();
        }

        private void btnResetPhoto_Click(object sender, EventArgs e)
        {
            this.actualImage = this.storedImage;
            PhotoImage.Source = actualImage;
            setPhotoButtonStates(true, false, false, false);
        }

        /// <summary>
        /// Click event handler for the crop button on the application bar.
        /// This code creates the new cropped writeable bitmap object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCrop_Click(object sender, EventArgs e)
        {

            //if (this.actualImage == null)
            //    return;

            //// Get the size of the source image prepared for cropping
            //double originalImageWidth = this.actualImage.PixelWidth;
            //double originalImageHeight = this.actualImage.PixelHeight;


            //// Get the size of the image when it is displayed on the phone
            //double displayedWidth = PhotoImage.ActualWidth;
            //double displayedHeight = PhotoImage.ActualHeight;

            //// Calculate the ratio of the original image to the displayed image
            //double widthRatio = originalImageWidth / displayedWidth;
            //double heightRatio = originalImageHeight / displayedHeight;

            //// Create a new WriteableBitmap. The size of the bitmap is the size of the cropping rectangle
            //// drawn by the user, multiplied by the image size ratio.
            //WriteableBitmap workImage = new WriteableBitmap((int)(widthRatio * Math.Abs(p2.X - p1.X)), (int)(heightRatio * Math.Abs(p2.Y - p1.Y)));


            //// Calculate the offset of the cropped image. This is the distance, in pixels, to the top left corner
            //// of the cropping rectangle, multiplied by the image size ratio.
            //int xoffset = (int)(((p1.X < p2.X) ? p1.X : p2.X) * widthRatio);
            //int yoffset = (int)(((p1.Y < p2.Y) ? p1.Y : p2.Y) * heightRatio);

            //// Copy the pixels from the targeted region of the source image into the target image, 
            //// using the calculated offset
            //for (int i = 0; i < workImage.Pixels.Length; i++)
            //{
            //    int x = (int)((i % workImage.PixelWidth) + xoffset);
            //    int y = (int)((i / workImage.PixelWidth) + yoffset);
            //    workImage.Pixels[i] = this.actualImage.Pixels[y * this.actualImage.PixelWidth + x];
            //}
            ////Replace the croppedImage with the new generated workImage
            //this.actualImage = workImage;
            //PhotoImage.Source = actualImage;
            //rect.Visibility = Visibility.Collapsed;


            ////Enable  accept and reject buttons to save or discard current cropped image.
            ////Disable crop button until a new cropping region is selected.
            //setPhotoButtonStates(true, false, true, true);

            ////Instructional text
            //textStatus.Text = "Continue to crop image, accept, or reject.";
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (this.actualImage == null)
                return;
            this.storedImage = this.actualImage;
            saveCapturedImage();
            VM.Save.Execute(null);
            setPhotoButtonStates(false, false, false, false);
        }
        #endregion

        #region cropping helper

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //if (cropping)
            //{
            //    rect.SetValue(Canvas.LeftProperty, (p1.X < p2.X) ? p1.X : p2.X);
            //    rect.SetValue(Canvas.TopProperty, (p1.Y < p2.Y) ? p1.Y : p2.Y);
            //    rect.Width = (int)Math.Abs(p2.X - p1.X);
            //    rect.Height = (int)Math.Abs(p2.Y - p1.Y);
            //}
        }


        /// <summary>
        /// Click event handler for mouse move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PhotoImage_MouseMove(object sender, MouseEventArgs e)
        {
            //p2 = e.GetPosition(PhotoImage);
        }

        /// <summary>
        /// Click event handler for mouse left button up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PhotoImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //p2 = e.GetPosition(PhotoImage);
            //cropping = false;


        }

        /// <summary>
        /// Click event handler for mouse left button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void PhotoImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //btnCrop.IsEnabled = true;
            //p1 = e.GetPosition(PhotoImage);
            //p2 = p1;
            //rect.Visibility = Visibility.Visible;
            //cropping = true;
        }

        /// <summary>
        /// Click event handler for the reject button on the application bar.
        /// This will allow you to reject the cropped image and set back to the original image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        #endregion

        private void saveCapturedImage()
        {
            //Save Picture
            //

            //Make progress bar visible for the event handler as there may be posible latency.
            progressBar1.Visibility = Visibility.Visible;

            //Create filename for JPEG in isolated storage as a Guid and filename for thumb
            String uri;

            if (VM.Current.Model.Uri == null || VM.Current.Model.Uri.Equals(String.Empty))
            {
                Guid g = Guid.NewGuid();
                uri = g.ToString() + ".jpg";
            }
            else
            {
                uri = VM.Current.Model.Uri;
            }
            //Create virtual store and file stream. Check for duplicate tempJPEG files.
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(uri))
            {
                myStore.DeleteFile(uri);
            }
            IsolatedStorageFileStream myFileStream = myStore.CreateFile(uri);
            //Encode the WriteableBitmap into JPEG stream and place into isolated storage.
            System.Windows.Media.Imaging.Extensions.SaveJpeg(this.storedImage, myFileStream, this.storedImage.PixelWidth, this.storedImage.PixelHeight, 0, 85);
            myFileStream.Close();
            if (VM.Current.Model.Uri == null || VM.Current.Model.Uri.Equals(String.Empty))
            {
                VM.Uri = uri;
                
            }
            progressBar1.Visibility = Visibility.Collapsed;
        }


        private void setPhotoBar()
        {
            progressBar1.Visibility = Visibility.Visible;
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            
            //This code creates the application bar icon buttons.
            btnCamera = new ApplicationBarIconButton(new Uri("/Images/appbar.feature.camera.rest.png", UriKind.Relative));
            btnCrop = new ApplicationBarIconButton(new Uri("/Images/appbar.edit.rest.png", UriKind.Relative));
            btnReset = new ApplicationBarIconButton(new Uri("/Images/appbar.refresh.rest.png", UriKind.Relative));
            btnSave = new ApplicationBarIconButton(new Uri("/Images/appbar.save.rest.png", UriKind.Relative));

            //Labels for the application bar buttons.
            btnCamera.Text = "camera";
            btnCrop.Text = "crop";
            btnReset.Text = "reset";
            btnSave.Text = "save";

            //This code adds buttons to application bar.
            ApplicationBar.Buttons.Add(btnCamera);
            ApplicationBar.Buttons.Add(btnCrop);
            ApplicationBar.Buttons.Add(btnReset);
            ApplicationBar.Buttons.Add(btnSave);

            
            //This code will create event handlers for buttons.
            btnCamera.Click += new EventHandler(btnCaptureImage_Click);
            btnCrop.Click += new EventHandler(btnCrop_Click);
            btnReset.Click += new EventHandler(btnResetPhoto_Click);
            btnSave.Click += new EventHandler(btnSaveImage_Click);
            progressBar1.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Helper method to change the IsEnabled property for the ApplicationBarIconButtons.
        /// </summary>
        /// <param name="cameraEnabled">New state for the camera button.</param>
        /// <param name="cropEnabled">New state for the crop button.</param>
        /// <param name="resetEnabled">New state for the reset button.</param>
         /// <param name="saveEnabled">New state for the save button.</param>
        private void setPhotoButtonStates(bool cameraEnabled, bool cropEnabled, bool resetEnabled, bool saveEnabled)
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = cameraEnabled;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = cropEnabled;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = resetEnabled;
            (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = saveEnabled;
        }


        #endregion


        #region Audio

        #region ViewConstruction

        private void NewAudio_Click(object sender, EventArgs e)
        {
            this.PageTitle.Text = "audio";
            VM.Current.Model.MediaType = MediaType.Audio;
            AudioStatusImage.Visibility = Visibility.Visible;
            // Timer to simulate the XNA Framework game loop (Microphone is 
            // from the XNA Framework). We also use this timer to monitor the 
            // state of audio playback so we can update the UI appropriately.
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            // Event handler for getting audio data when the buffer is full
            microphone.BufferReady += new EventHandler<EventArgs>(microphone_BufferReady);

            setAudioBar();

            blankImage = new BitmapImage(new Uri("/Images/AudioIcons/blank.png", UriKind.RelativeOrAbsolute));
            microphoneImage = new BitmapImage(new Uri("/Images/AudioIcons/microphone.png", UriKind.RelativeOrAbsolute));
            speakerImage = new BitmapImage(new Uri("/Images/AudioIcons/speaker.png", UriKind.RelativeOrAbsolute));
        }


        private void setAudioBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;
            

            //This code creates the application bar icon buttons.
            btnRecord = new ApplicationBarIconButton(new Uri("/Images/AudioIcons/record.png", UriKind.Relative));
            btnPlay= new ApplicationBarIconButton(new Uri("/Images/AudioIcons/play.png", UriKind.Relative));
            btnStop = new ApplicationBarIconButton(new Uri("/Images/AudioIcons/stop.png", UriKind.Relative));
            btnSave = new ApplicationBarIconButton(new Uri("/Images/appbar.save.rest.png", UriKind.Relative));

            //Labels for the application bar buttons.
            btnRecord.Text = "record";
            btnPlay.Text = "play";
            btnStop.Text = "stop";
            btnSave.Text = "save";

            //This code adds buttons to application bar.
            ApplicationBar.Buttons.Add(btnRecord);
            ApplicationBar.Buttons.Add(btnPlay);
            ApplicationBar.Buttons.Add(btnStop);
            ApplicationBar.Buttons.Add(btnSave);

            setAudioButtonStates(true, false, false, false);
            //This code will create event handlers for buttons.
            btnRecord.Click += new EventHandler(btnRecordAudio_Click);
            btnPlay.Click += new EventHandler(btnPlayAudio_Click);
            btnStop.Click += new EventHandler(btnStopAudio_Click);
            btnSave.Click += new EventHandler(btnSaveAudio_Click);
        }

        /// <summary>
        /// Helper method to change the IsEnabled property for the ApplicationBarIconButtons.
        /// </summary>
        /// <param name="recordEnabled">New state for the record button.</param>
        /// <param name="playEnabled">New state for the play button.</param>
        /// <param name="stopEnabled">New state for the stop button.</param>
        private void setAudioButtonStates(bool recordEnabled, bool playEnabled, bool stopEnabled, bool saveEnabled)
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = recordEnabled;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = playEnabled;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = stopEnabled;
            (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = saveEnabled;
        }

        #endregion

        #region audiohelper

        /// <summary>
        /// The Microphone.BufferReady event handler.
        /// Gets the audio data from the microphone and stores it in a buffer,
        /// then writes that buffer to a stream for later playback.
        /// Any action in this event handler should be quick!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void microphone_BufferReady(object sender, EventArgs e)
        {
            // Retrieve audio data
            microphone.GetData(this.audioBuffer);

            // Store the audio data in a stream
            audioStream.Write(audioBuffer, 0, audioBuffer.Length);
        }

        public void WriteWavHeader(Stream stream, int sampleRate)
        {
            const int bitsPerSample = 16;
            const int bytesPerSample = bitsPerSample / 8;
            var encoding = System.Text.Encoding.UTF8;

            // ChunkID Contains the letters "RIFF" in ASCII form (0x52494646 big-endian form).
            stream.Write(encoding.GetBytes("RIFF"), 0, 4);

            // NOTE this will be filled in later
            stream.Write(BitConverter.GetBytes(0), 0, 4);

            // Format Contains the letters "WAVE"(0x57415645 big-endian form).
            stream.Write(encoding.GetBytes("WAVE"), 0, 4);

            // Subchunk1ID Contains the letters "fmt " (0x666d7420 big-endian form).
            stream.Write(encoding.GetBytes("fmt "), 0, 4);

            // Subchunk1Size 16 for PCM.  This is the size of therest of the Subchunk which follows this number.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // AudioFormat PCM = 1 (i.e. Linear quantization) Values other than 1 indicate some form of compression.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // NumChannels Mono = 1, Stereo = 2, etc.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // SampleRate 8000, 44100, etc.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // ByteRate =  SampleRate * NumChannels * BitsPerSample/8
            stream.Write(BitConverter.GetBytes(sampleRate * bytesPerSample), 0, 4);

            // BlockAlign NumChannels * BitsPerSample/8 The number of bytes for one sample including all channels.
            stream.Write(BitConverter.GetBytes((short)(bytesPerSample)), 0, 2);

            // BitsPerSample    8 bits = 8, 16 bits = 16, etc.
            stream.Write(BitConverter.GetBytes((short)(bitsPerSample)), 0, 2);

            // Subchunk2ID Contains the letters "data" (0x64617461 big-endian form).
            stream.Write(encoding.GetBytes("data"), 0, 4);

            // NOTE to be filled in later
            stream.Write(BitConverter.GetBytes(0), 0, 4);
        }

        public void UpdateWavHeader(Stream stream)
        {
            if (!stream.CanSeek) throw new Exception("Can't seek stream to update wav header");

            var oldPos = stream.Position;

            // ChunkSize  36 + SubChunk2Size
            stream.Seek(4, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 8), 0, 4);

            // Subchunk2Size == NumSamples * NumChannels * BitsPerSample/8 This is the number of bytes in the data.
            stream.Seek(40, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 44), 0, 4);

            stream.Seek(oldPos, SeekOrigin.Begin);
        }

        /// <summary>
        /// Updates the XNA FrameworkDispatcher and checks to see if a sound is playing.
        /// If sound has stopped playing, it updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            try { FrameworkDispatcher.Update(); }
            catch { }

            if (true == soundIsPlaying)
            {
                if (soundInstance.State != SoundState.Playing)
                {
                    // Audio has finished playing
                    soundIsPlaying = false;

                    // Update the UI to reflect that the 
                    // sound has stopped playing
                    setAudioButtonStates(true, true, false, true);
                    AudioStatusImage.Source = blankImage;
                }
            }
        }

        /// <summary>
        /// Plays the audio using SoundEffectInstance 
        /// so we can monitor the playback status.
        /// </summary>
        private void playSound()
        {
            // Play audio using SoundEffectInstance so we can monitor it's State 
            // and update the UI in the dt_Tick handler when it is done playing.
            SoundEffect sound = new SoundEffect(audioStream.ToArray(), microphone.SampleRate, AudioChannels.Mono);
            soundInstance = sound.CreateInstance();
            soundIsPlaying = true;
            soundInstance.Play();
        }

        #endregion

        #region AudioButtons
        /// <summary>
        /// Handles the Click event for the record button.
        /// Sets up the microphone and data buffers to collect audio data,
        /// then starts the microphone. Also, updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRecordAudio_Click(object sender, EventArgs e)
        {
            // Get audio data in 1/2 second chunks
            microphone.BufferDuration = TimeSpan.FromMilliseconds(500);

            // Allocate memory to hold the audio data
            audioBuffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];

            // Set the stream back to zero in case there is already something in it
            audioStream.SetLength(0);

            // Start recording
            microphone.Start();
            WriteWavHeader(audioStream, microphone.SampleRate);
            setAudioButtonStates(false, false, true,false);
            AudioStatusImage.Source = microphoneImage;
        }

        /// <summary>
        /// Handles the Click event for the stop button.
        /// Stops the microphone from collecting audio and updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopAudio_Click(object sender, EventArgs e)
        {
            if (microphone.State == MicrophoneState.Started)
            {
                // In RECORD mode, user clicked the 
                // stop button to end recording
                microphone.Stop();

            }
            else if (soundInstance.State == SoundState.Playing)
            {
                // In PLAY mode, user clicked the 
                // stop button to end playing back
                soundInstance.Stop();
            }

            setAudioButtonStates(true, true, false, true);
            AudioStatusImage.Source = blankImage;
        }


       

        /// <summary>
        /// Handles the Click event for the play button.
        /// Plays the audio collected from the microphone and updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlayAudio_Click(object sender, EventArgs e)
        {
            if (audioStream.Length > 0)
            {
                // Update the UI to reflect that
                // sound is playing
                setAudioButtonStates(false, false, true, false);
                AudioStatusImage.Source = speakerImage;

                // Play the audio in a new thread so the UI can update.
                Thread soundThread = new Thread(new ThreadStart(playSound));
                soundThread.Start();
            }
        }

        private void btnSaveAudio_Click(object sender, EventArgs e)
        {
            saveAudio();
            VM.Save.Execute(null);
            setAudioButtonStates(false, false, false, false);
        }

        #endregion

        private void saveAudio()
        {
            //Save Audio
            //

            //Make progress bar visible for the event handler as there may be posible latency.
            progressBar1.Visibility = Visibility.Visible;

            //Create filename for JPEG in isolated storage as a Guid and filename for thumb
            String uri;

            if (VM.Current.Model.Uri == null || VM.Current.Model.Uri.Equals(String.Empty))
            {
                Guid g = Guid.NewGuid();
                uri = g.ToString() + ".wav";
            }
            else
            {
                uri = VM.Current.Model.Uri;
            }
            //Create virtual store and file stream. Check for duplicate tempJPEG files.
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(uri))
            {
                myStore.DeleteFile(uri);
            }
            UpdateWavHeader(audioStream);
            IsolatedStorageFileStream myFileStream = myStore.CreateFile(uri);
            audioStream.WriteTo(myFileStream);
            myFileStream.Flush();

            myFileStream.Close();
            if (VM.Current.Model.Uri == null || VM.Current.Model.Uri.Equals(String.Empty))
            {
                VM.Uri = uri;

            }
            progressBar1.Visibility = Visibility.Collapsed;
        }

        #endregion


        #region Video

        #region ViewConstruction

        private void NewVideo_Click(object sender, EventArgs e)
        {
            this.PageTitle.Text = "video";
            VM.Current.Model.MediaType = MediaType.Video;
            setVideoBar();
            setVideoButtonStates(true, false, false, false);
            VideoPlayer.Visibility = Visibility.Visible;
            viewfinderRectangle.Visibility = Visibility.Visible;
            // Initialize the video recorder.
            InitializeVideoRecorder();
        }

        private void setVideoBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;

            //This code creates the application bar icon buttons.
            btnRecord = new ApplicationBarIconButton(new Uri("/Images/appbar.feature.video.rest.png", UriKind.Relative));
            btnPlay = new ApplicationBarIconButton(new Uri("/Images/AudioIcons/play.png", UriKind.Relative));
            btnStop = new ApplicationBarIconButton(new Uri("/Images/AudioIcons/stop.png", UriKind.Relative));
            btnSave = new ApplicationBarIconButton(new Uri("/Images/appbar.save.rest.png", UriKind.Relative));

            //Labels for the application bar buttons.
            btnRecord.Text = "record";
            btnPlay.Text = "play";
            btnStop.Text = "stop";
            btnSave.Text = "save";

            //This code adds buttons to application bar.
            ApplicationBar.Buttons.Add(btnRecord);
            ApplicationBar.Buttons.Add(btnPlay);
            ApplicationBar.Buttons.Add(btnStop);
            ApplicationBar.Buttons.Add(btnSave);

            setVideoButtonStates(true, false, false, false);
            //This code will create event handlers for buttons.
            btnRecord.Click += new EventHandler(btnRecordVideo_Click);
            btnPlay.Click += new EventHandler(btnPlayVideo_Click);
            btnStop.Click += new EventHandler(btnStopVideo_Click);
            btnSave.Click += new EventHandler(btnSaveVideo_Click);
        }

        private void setVideoButtonStates(bool recordEnabled, bool playEnabled, bool stopEnabled, bool saveEnabled)
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = recordEnabled;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = playEnabled;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = stopEnabled;
            (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = saveEnabled;
        }

        #endregion

        #region Buttons

        private void btnRecordVideo_Click(object sender, EventArgs e)
        {
            setVideoButtonStates(false, false, false, false);
            StartVideoRecording();
        }

        private void btnPlayVideo_Click(object sender, EventArgs e)
        {
            // Avoid duplicate taps.
            setVideoButtonStates(false, false, false, false);

            // Start video playback when the file stream exists.
            if (isoVideoFile != null)
            {
                VideoPlayer.Play();
            }
            // Start the video for the first time.
            else
            {
                // Stop the capture source.
                captureSource.Stop();

                // Remove VideoBrush from the tree.
                viewfinderRectangle.Fill = null;

                // Create the file stream and attach it to the MediaElement.
                isoVideoFile = new IsolatedStorageFileStream(isoVideoFileName,
                                        FileMode.Open, FileAccess.Read,
                                        IsolatedStorageFile.GetUserStoreForApplication());

                VideoPlayer.SetSource(isoVideoFile);

                // Add an event handler for the end of playback.
                VideoPlayer.MediaEnded += new RoutedEventHandler(VideoPlayerMediaEnded);

                // Start video playback.
                VideoPlayer.Play();
            }

            // Set the button state and the message.
            setVideoButtonStates(false, false, true, false);
        }

        private void btnStopVideo_Click(object sender, EventArgs e)
        {
            // Avoid duplicate taps.
            setVideoButtonStates(false, false, false, false);

            // Stop during video recording.
            if (this.isVideoRecording == true)
            {
                StopVideoRecording();

                // Set the button state and the message.
                setVideoButtonStates(true, true, false, true);
            }

            // Stop during video playback.
            else
            {
                // Remove playback objects.
                DisposeVideoPlayer();

                StartVideoPreview();

                // Set the button state and the message.
                setVideoButtonStates(true, true, false, true);
            }
        }

        private void btnSaveVideo_Click(object sender, EventArgs e)
        {
            saveVideo();
            VM.Save.Execute(null);
            setAudioButtonStates(false, false, false, false);
        }

        #endregion



        #region Dispose
        private void DisposeVideoPlayer()
        {
            if (videoPlayer != null)
            {
                // Stop the VideoPlayer MediaElement.
                videoPlayer.Stop();

                // Remove playback objects.
                videoPlayer.Source = null;

                // Remove the event handler.
                videoPlayer.MediaEnded -= VideoPlayerMediaEnded;
            }
        }

        private void DisposeVideoRecorder()
        {
            if (captureSource != null)
            {
                // Stop captureSource if it is running.
                if (captureSource.VideoCaptureDevice != null
                    && captureSource.State == CaptureState.Started)
                {
                    captureSource.Stop();
                }

                // Remove the event handlers for capturesource and the shutter button.
                captureSource.CaptureFailed -= OnCaptureFailed;

                // Remove the video recording objects.
                captureSource = null;
                videoCaptureDevice = null;
                fileSink = null;
                videoRecorderBrush = null;
            }
        }
        #endregion


        private void saveVideo()
        {
            //Save Video
            //

            //Make progress bar visible for the event handler as there may be posible latency.
            progressBar1.Visibility = Visibility.Visible;

            //Create filename for JPEG in isolated storage as a Guid and filename for thumb
            String uri;

            if (VM.Current.Model.Uri == null || VM.Current.Model.Uri.Equals(String.Empty))
            {
                Guid g = Guid.NewGuid();
                uri = g.ToString() + ".mp4";
            }
            else
            {
                uri = VM.Current.Model.Uri;
            }
            //Create virtual store and file stream. Check for duplicate tempJPEG files.
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(uri))
            {
                myStore.DeleteFile(uri);
            }
            if(myStore.FileExists(isoVideoFileName))
            {
                myStore.MoveFile(isoVideoFileName,uri);
            }
            if (VM.Current.Model.Uri == null || VM.Current.Model.Uri.Equals(String.Empty))
            {
                VM.Uri = uri;

            }
            progressBar1.Visibility = Visibility.Collapsed;
        }



        #region VideoMethods


        public void InitializeVideoRecorder()
        {
            if (captureSource == null)
            {
                // Create the VideoRecorder objects.
                captureSource = new CaptureSource();
                fileSink = new FileSink();

                videoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();

                // Add eventhandlers for captureSource.
                captureSource.CaptureFailed += new EventHandler<ExceptionRoutedEventArgs>(OnCaptureFailed);

                // Initialize the camera if it exists on the device.
                if (videoCaptureDevice != null)
                {
                    // Create the VideoBrush for the viewfinder.
                    videoRecorderBrush = new VideoBrush();
                    videoRecorderBrush.SetSource(captureSource);

                    // Display the viewfinder image on the rectangle.
                    viewfinderRectangle.Fill = videoRecorderBrush;

                    // Start video capture and display it on the viewfinder.
                    captureSource.Start();
                    setVideoButtonStates(true, false, false, false);
                }
                else
                {
                    // Disable buttons when the camera is not supported by the device.
                    setVideoButtonStates(false, false, false, false);
                }
            }
        }

        // If recording fails, display an error message.
        private void OnCaptureFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(delegate()
            {
                textStatus.Text = "ERROR: " + e.ErrorException.Message.ToString();
            });
        }

        // Display the viewfinder when playback ends.
        public void VideoPlayerMediaEnded(object sender, RoutedEventArgs e)
        {
            // Remove the playback objects.
            DisposeVideoPlayer();
            setVideoButtonStates(true, true, false, true);
            StartVideoPreview();
        }

          // Set the recording state: display the video on the viewfinder.
        private void StartVideoPreview()
        {
            try
            {
                // Display the video on the viewfinder.
                if (captureSource.VideoCaptureDevice != null
                && captureSource.State == CaptureState.Stopped)
                {
                    // Add captureSource to videoBrush.
                    videoRecorderBrush.SetSource(captureSource);

                    // Add videoBrush to the visual tree.
                    viewfinderRectangle.Fill = videoRecorderBrush;

                    captureSource.Start();
                }
            }
            // If preview fails, display an error.
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    textStatus.Text = "ERROR: " + e.Message.ToString();
                });
            }
        }

        // Set recording state: start recording.
        private void StartVideoRecording()
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
                this.isVideoRecording = true;
                setVideoButtonStates(false, false, true, false);
            }

            // If recording fails, display an error.
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    textStatus.Text = "ERROR: " + e.Message.ToString();
                });
            }
        }

        // Set the recording state: stop recording.
        private void StopVideoRecording()
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

                }
                this.isVideoRecording = false;
                setVideoButtonStates(true, true, false, true);
            }
            // If stop fails, display an error.
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                   textStatus.Text = "ERROR: " + e.Message.ToString();
                });
                this.isVideoRecording = false;
            }
        }
        #endregion

        #endregion
    }
}