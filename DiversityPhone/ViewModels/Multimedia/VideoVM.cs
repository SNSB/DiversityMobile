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
    public class VideoVM : EditPageVMBase<MultimediaObject>, IAudioVideoPageVM
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


        #endregion

        #region Commands


        public IReactiveCommand Record { get; private set; }
        public IReactiveCommand Play { get; private set; }
        public IReactiveCommand Stop { get; private set; }

        #endregion      

        public VideoVM()
            : base( mmo => mmo.MediaType == MediaType.Video)
        {
            Record = new ReactiveCommand(this.ObservableForProperty(x => x.IsEditable).Value());
            Play = new ReactiveCommand();
            Stop = new ReactiveCommand();

            CanSave().Subscribe(CanSaveSubject);
        }        

        protected override void UpdateModel()
        {
            saveVideo();            
            Current.Model.Uri = Uri;
        }

        protected IObservable<bool> CanSave()
        {
            var idle = this.ObservableForProperty(x => x.State)
                .Select(sound => sound.Value == PlayStates.Idle)
                .StartWith(false);

            var recordPresent = this.ObservableForProperty(x => x.RecordPresent)
                .Select(present => present.Value)
                .StartWith(false);
            return idle.BooleanAnd(recordPresent);
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
            if (myStore.FileExists(TempFileName))
            {
                myStore.MoveFile(TempFileName, uri);
            }
            if (Current.Model.Uri == null || Current.Model.Uri.Equals(String.Empty))
            {
                Uri = uri;
            }
        }
    }
}
