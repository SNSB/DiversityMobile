using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Windows.Threading;

namespace DiversityPhone.ViewModels
{
    public class ViewAudioVM : EditPageVMBase<MultimediaObject>, IAudioVideoPageVM
    {
        #region Commands
        public IReactiveCommand Play { get; protected set; }
        public IReactiveCommand Stop { get; protected set; }

        #endregion

        public DynamicSoundEffectInstance sound;
        int position;
        int count;
        byte[] byteArray;
        public BitmapImage blankImage;
        public BitmapImage speakerImage;
        

        #region Properties
        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }

        private BitmapImage _statusImage;
        public BitmapImage StatusImage
        {
            get { return _statusImage; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.StatusImage, ref _statusImage, value);
            }
        }
        


        private bool _soundIsPlaying = false;
        public bool SoundIsPlaying
        {
            get { return _soundIsPlaying; }
            set { this.RaiseAndSetIfChanged(x => x.SoundIsPlaying, ref _soundIsPlaying, value); }
        }


        #endregion


        public ViewAudioVM()
            : base(mmo => mmo.MediaType == MediaType.Audio)         
        {
            Play = new ReactiveCommand();
            Stop = new ReactiveCommand();

            CurrentModelObservable
              .Select(mmo => mmo.Uri)
              .BindTo(this, x => x.Uri);


            Play.Subscribe(_ => playSound());
            Stop.Subscribe(_ => stopPlaying());
            blankImage = new BitmapImage(new Uri("/Images/AudioIcons/blank.png", UriKind.RelativeOrAbsolute));
            speakerImage = new BitmapImage(new Uri("/Images/AudioIcons/speaker.png", UriKind.RelativeOrAbsolute));
            StatusImage = blankImage;
        }

        #region Audio


       


        private void playSound()
        {
           if(sound==null)
                LoadContent();
            sound.Play();
            StatusImage = speakerImage;
            SoundIsPlaying = true;
        }

        private void stopPlaying()
        {
            sound.Stop();
            position = 0;
            StatusImage = blankImage;
            SoundIsPlaying = false;
        }

        protected  void LoadContent()
        {
            if (Uri == null)
                return;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                if (!isf.FileExists(Uri))
                    return;
                System.IO.Stream waveFileStream = isf.OpenFile(Uri,FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(waveFileStream);

                int chunkID = reader.ReadInt32();
                int fileSize = reader.ReadInt32();
                int riffType = reader.ReadInt32();
                int fmtID = reader.ReadInt32();
                int fmtSize = reader.ReadInt32();
                int fmtCode = reader.ReadInt16();
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int fmtAvgBPS = reader.ReadInt32();
                int fmtBlockAlign = reader.ReadInt16();
                int bitDepth = reader.ReadInt16();

                if (fmtSize == 18)
                {
                    // Read any extra values
                    int fmtExtraSize = reader.ReadInt16();
                    reader.ReadBytes(fmtExtraSize);
                }

                int dataID = reader.ReadInt32();
                int dataSize = reader.ReadInt32();

                byteArray = reader.ReadBytes(dataSize);

                sound = new DynamicSoundEffectInstance(sampleRate, (AudioChannels)channels);

                count = sound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(100));

                sound.BufferNeeded += new EventHandler<EventArgs>(DynamicSound_BufferNeeded);
            }
        }

        public void UnloadContent()
        {
            if(sound!=null)
                 sound.Dispose();
        }



        void DynamicSound_BufferNeeded(object sender, EventArgs e)
        {
            sound.SubmitBuffer(byteArray, position, count / 2);
            sound.SubmitBuffer(byteArray, position + count / 2, count / 2);

            position += count;
            if (position + count > byteArray.Length)
            {
                sound.Stop();
                position = 0;
            }
        }
       


        #endregion        
    }
}
