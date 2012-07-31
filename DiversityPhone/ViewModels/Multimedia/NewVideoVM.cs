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
       

        #region Properties

        public string TempFileName 
        {
            get { return "tempVideoSave.mp4"; }
        }

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

        //private Brush _Fill = null;
        //public Brush Fill
        //{
        //    get
        //    {
        //        return _Fill;
        //    }
        //    set
        //    {
        //        this.RaiseAndSetIfChanged(x => x.Fill, ref _Fill, value);
        //    }
        //}

        #endregion

        #region Commands


        public ReactiveCommand Record { get; private set; }
        public ReactiveCommand Play { get; private set; }
        public ReactiveCommand Stop { get; private set; }

        #endregion

        #region Constructor

        public NewVideoVM()
        {
            Record = new ReactiveCommand();
            
            Play = new ReactiveCommand();
            Stop = new ReactiveCommand();
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
            return null;
        }

        #endregion

        #region Methods for ReactiveCommands

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
            if (myStore.FileExists(TempFileName))
            {
                myStore.MoveFile(TempFileName, uri);
            }
            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                Uri = uri;
            }
        }
        #endregion

        #region Video

        #region Recording

       

        #endregion


       

        //public void initializeVideoPlayer()
        //{
        //    videoPlayer = new MediaElement();
        //    videoPlayer.Width=656;
        //    videoPlayer.Height=480;
        //    videoPlayer.AutoPlay=true;
        //    videoPlayer.RenderTransformOrigin=new Point(0.5,0.5);
        //    videoPlayer.VerticalAlignment=VerticalAlignment.Center;
        //    videoPlayer.HorizontalAlignment=HorizontalAlignment.Center;
        //    videoPlayer.Stretch=Stretch.Fill;
        //}

      


        #endregion


    }
}
