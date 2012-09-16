using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DiversityPhone.Model;
using DiversityPhone.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReactiveUI;
using ReactiveUI.Xaml;


namespace DiversityPhone.ViewModels
{
    public class AudioVM : EditPageVMBase<MultimediaObject>
    {

      

        


        //Audio Components
        private Microphone microphone = Microphone.Default;
        private DispatcherTimer timer;
       
        private MemoryStream audioStream = new MemoryStream();  // Stores the audio data for later playback
        private SoundEffectInstance soundInstance;              // Used to play back audio
        
        private BitmapImage blankImage = new BitmapImage(new Uri("/Images/AudioIcons/blank.png", UriKind.RelativeOrAbsolute));
        private BitmapImage microphoneImage = new BitmapImage(new Uri("/Images/AudioIcons/microphone.png", UriKind.RelativeOrAbsolute));
        private BitmapImage speakerImage = new BitmapImage(new Uri("/Images/AudioIcons/speaker.png", UriKind.RelativeOrAbsolute));

        #region Properties


        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }

        private byte[] _audioBuffer;                             // Dynamic buffer to retrieve audio data from the microphone
        public byte[] AudioBuffer
        {
            get
            {
                return _audioBuffer;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.AudioBuffer, ref _audioBuffer, value);
            }
        }

        private BitmapImage _audioStatusImage;

        public BitmapImage AudioStatusImage
        {
            get
            {
                return _audioStatusImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.AudioStatusImage, ref _audioStatusImage, value);
            }
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

       

        #endregion


        #region Commands


        public ReactiveCommand Record { get; private set; }
        public ReactiveCommand Play { get; private set; }
        public ReactiveCommand Stop { get; private set; }

        #endregion

        #region Constructor

        public AudioVM()
            : base( mmo => mmo.MediaType == MediaType.Audio)
        {
            // Timer to simulate the XNA Framework game loop (Microphone is 
            // from the XNA Framework). We also use this timer to monitor the 
            // state of audio playback so we can update the UI appropriately.
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            // Event handler for getting audio data when the buffer is full
            microphone.BufferReady += new EventHandler<EventArgs>(microphone_BufferReady);

            Record = new ReactiveCommand();
            Record.Subscribe(_ => recordAudio());
            Play = new ReactiveCommand();
            Play.Subscribe(_ => playAudio());
            Stop = new ReactiveCommand();
            Stop.Subscribe(_ => stop());

            CanSave().Subscribe(CanSaveSubject);
        }

        #endregion

        #region Inherited

        protected override void UpdateModel()
        {
            saveAudio();            
            Current.Model.Uri = Uri;
        }       

        protected IObservable<bool> CanSave()
        {
            var idle = this.ObservableForProperty(x => x.State)
                .Select(sound => sound.Value==PlayStates.Idle)
                .StartWith(false);

            var bufferReady = this.ObservableForProperty(x => x.AudioBuffer)
                .Select(buffer => buffer.Value != null)
                .StartWith(false);
            return idle.BooleanAnd(bufferReady);
        }       

        #endregion

        #region WavHandling

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

        #endregion

        #region Events

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
            microphone.GetData(this.AudioBuffer);

            // Store the audio data in a stream
            audioStream.Write(AudioBuffer, 0, AudioBuffer.Length);
        }

       

        /// <summary>
        /// Updates the XNA FrameworkDispatcher and checks to see if a sound is playing.
        /// If sound has stopped playing, it updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            try { FrameworkDispatcher.Update(); }
            catch { }

            if (State==PlayStates.Playing)
            {
                if (soundInstance.State != SoundState.Playing)
                {
                    // Audio has finished playing
                    State = PlayStates.Idle;

                    // Update the UI to reflect that the 
                    // sound has stopped playing
                    AudioStatusImage = blankImage;
                }
            }
        }


        #endregion


        #region Methods for ReactiveCommands

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
            State = PlayStates.Playing;
            soundInstance.Play();
        }

        private void playAudio()
        {
            if (audioStream.Length > 0)
            {
                AudioStatusImage = speakerImage;

                // Play the audio in a new thread so the UI can update.
                Thread soundThread = new Thread(new ThreadStart(playSound));
                soundThread.Start();
            }
        }


        private void recordAudio()
        {
            microphone.BufferDuration = TimeSpan.FromMilliseconds(500);
            AudioBuffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];
            audioStream.SetLength(0);
            State = PlayStates.Recording;
            microphone.Start();
            WriteWavHeader(audioStream, microphone.SampleRate);
            AudioStatusImage = microphoneImage;
        }

        private void stop()
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
            State = PlayStates.Idle;
            AudioStatusImage = blankImage;
        }


        private void saveAudio()
        {
            //Create filename for JPEG in isolated storage as a Guid and filename for thumb
            String uri;

            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                Guid g = Guid.NewGuid();
                uri = g.ToString() + ".wav";
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
            UpdateWavHeader(audioStream);
            IsolatedStorageFileStream myFileStream = myStore.CreateFile(uri);
            audioStream.WriteTo(myFileStream);
            myFileStream.Flush();

            myFileStream.Close();
            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                Uri = uri;
            }
        }

        #endregion

    }
}
