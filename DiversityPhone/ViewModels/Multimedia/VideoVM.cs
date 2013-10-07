using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.IO.IsolatedStorage;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class VideoVM : EditPageVMBase<MultimediaObject>, IAudioVideoPageVM
    {
        public readonly IStoreMultimedia VideoStore;

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

        public VideoVM(IStoreMultimedia VideoStore)
            : base(mmo => mmo.MediaType == MediaType.Video)
        {
            this.VideoStore = VideoStore;

            Record = new ReactiveCommand(this.ObservableForProperty(x => x.IsEditable).Value());
            Play = new ReactiveCommand(this.ObservableForProperty(x => x.RecordPresent).Value().StartWith(false));
            Stop = new ReactiveCommand();

            ModelByVisitObservable
                .Subscribe(m =>
                    {
                        RecordPresent = false;
                        State = PlayStates.Idle;
                        Uri = m.Uri;
                    });

            CanSave().Subscribe(CanSaveSubject);
        }

        protected override void UpdateModel()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(TempFileName))
                {
                    using (var fileStream = store.OpenFile(TempFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        Current.Model.Uri = VideoStore.StoreMultimedia(Current.Model.NewFileName(), fileStream);
                    }
                    store.DeleteFile(TempFileName);
                }
            }
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


    }
}
