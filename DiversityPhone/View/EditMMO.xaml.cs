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
        private WriteableBitmap capturedImage;
        private CameraCaptureTask takePhoto;

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

        //ApplicationBarAdjustment
        ApplicationBarIconButton btnRecord;
        ApplicationBarIconButton btnPlay;
        ApplicationBarIconButton btnStop;
        ApplicationBarIconButton btnPause;
        ApplicationBarIconButton bntCrop;
        ApplicationBarIconButton btnCamera;


        public EditMMO()
        {
            InitializeComponent();
           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

     


      
        #region Photo

        private void NewPhoto_Click(object sender, EventArgs e)
        {
            this.PageTitle.Text = "photo";
            VM.Model.MediaType = MediaType.Image;
            //Create new instance of CameraCaptureClass
            takePhoto = new CameraCaptureTask();

            //Create new event handler for capturing a photo
            takePhoto.Completed += new EventHandler<PhotoResult>(takePhoto_Completed);


            textStatus.Text = "";

            //Show the camera.
            takePhoto.Show();

        }

        void takePhoto_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {
                if (VM == null)
                    return;
                //Set progress bar to visible to show time between user snapshot and decoding
                //of image into a writeable bitmap object.
                progressBar1.Visibility = Visibility.Visible;

                //Take JPEG stream and decode into a WriteableBitmap object
                this.capturedImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);

                //Collapse visibility on the progress bar once writeable bitmap is visible.
                progressBar1.Visibility = Visibility.Collapsed;


                //Populate image control with WriteableBitmap object.
                MainImage.Source = this.capturedImage;
                MainImage.Visibility = Visibility.Visible;

                //Adjust ApplicationBar
                this.ApplicationBar.IsVisible = false;

                //Save Picture
                //

                //Make progress bar visible for the event handler as there may be posible latency.
                progressBar1.Visibility = Visibility.Visible;

                //Create filename for JPEG in isolated storage as a Guid and filename for thumb
                Guid g = Guid.NewGuid();
                String uri = g.ToString()+".jpg";
                String thumbUri = "thumb" + uri;

                //Create virtual store and file stream. Check for duplicate tempJPEG files.
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(uri))
                {
                    myStore.DeleteFile(uri);
                }
                if (myStore.FileExists(thumbUri))
                    myStore.DeleteFile(thumbUri);
                IsolatedStorageFileStream myFileStream = myStore.CreateFile(uri);
                //Encode the WriteableBitmap into JPEG stream and place into isolated storage.
                System.Windows.Media.Imaging.Extensions.SaveJpeg(this.capturedImage, myFileStream, this.capturedImage.PixelWidth, this.capturedImage.PixelHeight, 0, 85);
                //Save Thumb
                myFileStream = myStore.CreateFile(thumbUri);
                int width=Math.Min(this.capturedImage.PixelWidth,80);
                double ratio=(double) this.capturedImage.PixelHeight/(double) this.capturedImage.PixelWidth;
                int height=(int) (width*ratio);
                System.Windows.Media.Imaging.Extensions.SaveJpeg(this.capturedImage, myFileStream, width, height, 0, 70);
                VM.Model.Uri = uri;
                myFileStream.Close();
                VM.Save.Execute(null);
                progressBar1.Visibility = Visibility.Collapsed;
            }
            else
            {
                textStatus.Text = "You decided not to take a picture.";
            }
        }
        #endregion


        #region Audio


        private void NewAudio_Click(object sender, EventArgs e)
        {
            this.PageTitle.Text = "audio";
            VM.Model.MediaType = MediaType.Audio;
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

            blankImage = new BitmapImage(new Uri("Images/AudioIcons/blank.png", UriKind.RelativeOrAbsolute));
            microphoneImage = new BitmapImage(new Uri("Images/AudioIcons/microphone.png", UriKind.RelativeOrAbsolute));
            speakerImage = new BitmapImage(new Uri("Images/AudioIcons/speaker.png", UriKind.RelativeOrAbsolute));
        }


        private void setAudioBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsVisible = true;

            //This code creates the application bar icon buttons.
            btnRecord = new ApplicationBarIconButton(new Uri("/Images/AudioIcons/record.png", UriKind.Relative));
            btnPlay= new ApplicationBarIconButton(new Uri("/Images/AudioIcons/play.png", UriKind.Relative));
            btnStop = new ApplicationBarIconButton(new Uri("/Images/AudioIcons/stop.png", UriKind.Relative));

            //Labels for the application bar buttons.
            btnRecord.Text = "record";
            btnPlay.Text = "play";
            btnStop.Text = "stop";

            //This code adds buttons to application bar.
            ApplicationBar.Buttons.Add(btnRecord);
            ApplicationBar.Buttons.Add(btnPlay);
            ApplicationBar.Buttons.Add(btnStop);

            SetAudioButtonStates(true, false, false);
            //This code will create event handlers for buttons.
            btnRecord.Click += new EventHandler(btnRecord_Click);
            btnPlay.Click += new EventHandler(btnPlay_Click);
            btnStop.Click += new EventHandler(btnStop_Click);
        }

        /// <summary>
        /// Helper method to change the IsEnabled property for the ApplicationBarIconButtons.
        /// </summary>
        /// <param name="recordEnabled">New state for the record button.</param>
        /// <param name="playEnabled">New state for the play button.</param>
        /// <param name="stopEnabled">New state for the stop button.</param>
        private void SetAudioButtonStates(bool recordEnabled, bool playEnabled, bool stopEnabled)
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = recordEnabled;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = playEnabled;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = stopEnabled;
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
                    SetAudioButtonStates(true, true, false);
                    MainImage.Source = blankImage;
                    MainImage.Visibility = Visibility.Visible;
                }
            }
        }

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
        /// Handles the Click event for the record button.
        /// Sets up the microphone and data buffers to collect audio data,
        /// then starts the microphone. Also, updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRecord_Click(object sender, EventArgs e)
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
            SetAudioButtonStates(false, false, true);
            MainImage.Source = microphoneImage;
        }

        /// <summary>
        /// Handles the Click event for the stop button.
        /// Stops the microphone from collecting audio and updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (microphone.State == MicrophoneState.Started)
            {
                // In RECORD mode, user clicked the 
                // stop button to end recording
                microphone.Stop();

                //Save MemoryStream
                progressBar1.Visibility = Visibility.Visible;
                //Create filename fpr isolated storage
                if (VM.Model.Uri == null)
                {
                    Guid g = Guid.NewGuid();
                    String uri = g.ToString() + ".wav";
                    VM.Model.Uri = uri;
                }

                //Create virtual store and file stream. Check for duplicate tempJPEG files.
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(VM.Model.Uri))
                {
                    myStore.DeleteFile(VM.Model.Uri);
                }
                UpdateWavHeader(audioStream);
                IsolatedStorageFileStream myFileStream = myStore.CreateFile(VM.Model.Uri);
                audioStream.WriteTo(myFileStream);
                myFileStream.Flush();
                myFileStream.Close();
                VM.Save.Execute(null);
                progressBar1.Visibility = Visibility.Collapsed;

            }
            else if (soundInstance.State == SoundState.Playing)
            {
                // In PLAY mode, user clicked the 
                // stop button to end playing back
                soundInstance.Stop();
            }

            SetAudioButtonStates(true, true, false);
            MainImage.Source = blankImage;
        }

        /// <summary>
        /// Handles the Click event for the play button.
        /// Plays the audio collected from the microphone and updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (audioStream.Length > 0)
            {
                // Update the UI to reflect that
                // sound is playing
                SetAudioButtonStates(false, false, true);
                MainImage.Source = speakerImage;

                // Play the audio in a new thread so the UI can update.
                Thread soundThread = new Thread(new ThreadStart(playSound));
                soundThread.Start();
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


        #region Video

        private void NewVideo_Click(object sender, EventArgs e)
        {
            this.PageTitle.Text = "video";
            VM.Model.MediaType = MediaType.Video;
        }

        #endregion
    }
}