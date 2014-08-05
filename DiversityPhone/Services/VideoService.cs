namespace DiversityPhone.Services {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public enum PlayState {
        Paused,
        Playing,
        Recording,
        Previewing
    }

    public interface IVideoService {
        Stream GetRecording();
        void CreateRecording();
        void SetVideoFile(Stream video);
        IObservable<bool> HasRecording();
    }

    public class VideoService : IVideoService, IDisposable {
        // Current State
        PlayState _State;
        bool _CanRecord;
        string _RecordingFileName;

        // Capture and UI
        private Rectangle _ViewFinder;
        private MediaElement _Player;
        private Stream _OpenedVideo;
        private VideoBrush _Brush;
        private CaptureSource _CaptureSource;
        private FileSink _FileSink;

        // Notifications and Commands
        private ISubject<bool> _HasRecording;

        private SwitchableCommand _Record, _Play, _Stop;
        public ICommand Record { get { return _Record; } }
        public ICommand Play { get { return _Play; } }
        public ICommand Stop { get { return _Stop; } }

        public VideoService(
            Rectangle viewFinder,
            MediaElement player
            ) {
            // Initial State
            _State = PlayState.Paused;
            _CanRecord = false;

            _Record = new SwitchableCommand(OnRecord);
            _Play = new SwitchableCommand(OnPlay);
            _Stop = new SwitchableCommand(OnPause);


            _ViewFinder = viewFinder;

            _Player = player;
            _Player.MediaEnded += MediaEnded;

            _CaptureSource = new CaptureSource();
            _CaptureSource.CaptureFailed += CaptureFailed;

            _FileSink = new FileSink();
            _Brush = new VideoBrush();

            _HasRecording = new BehaviorSubject<bool>(false);
        }

        public Stream GetRecording() {
            return OpenRecording();
        }

        public IObservable<bool> HasRecording() {
            return _HasRecording;
        }

        public void CreateRecording() {
            if (_State == PlayState.Paused
                && !_CanRecord) {
                CloseVideoStream();

                _RecordingFileName = NewVideoFilePath();

                _HasRecording.OnNext(false);

                _CanRecord = true;
                UpdateUI(
                    PlayState.Previewing,
                    record: true,
                    play: false,
                    pause: false
                    );
            }
            else {
                // Invalid Transition
                Debugger.Break();
            }
        }

        public void SetVideoFile(Stream video) {
            if (_State == PlayState.Paused) {
                CloseVideoStream();
                _CanRecord = false;

                UpdateUI(PlayState.Playing,
                    record: false,
                    play: false,
                    pause: true
                    );

                OpenVideoStream(video);
            }
            else {
                // Invalid Transition
                Debugger.Break();
            }
        }

        private void OnRecord() {
            if (_State == PlayState.Previewing
                && _CanRecord) {
                // Close Handle to Recording File if any
                CloseVideoStream();

                UpdateUI(
                   PlayState.Recording,
                   record: false,
                   play: false,
                   pause: true
                   );

                try {
                    if (_CaptureSource.VideoCaptureDevice != null) {
                        // Connect File Sink to Capture Source
                        if (_CaptureSource.State == CaptureState.Started) {
                            _CaptureSource.Stop();
                        }
                        _FileSink.CaptureSource = _CaptureSource;
                        _FileSink.IsolatedStorageFileName = _RecordingFileName;
                        _CaptureSource.Start();
                    }
                }
                catch (Exception) {
                    // TODO Log
                }
            }
            else {
                // Invalid Transition
                Debugger.Break();
            }
        }

        private void OnPlay() {
            if ((_State == PlayState.Paused || _State == PlayState.Previewing)) {
                UpdateUI(PlayState.Playing,
                    record: false,
                    play: false,
                    pause: true
                    );

                _Player.Play();
            }
            else {
                // Invalid Transition
                Debugger.Break();
            }
        }

        private void OnPause() {
            if (_State == PlayState.Recording) {
                try {
                    // Stop recording.
                    if (_CaptureSource.VideoCaptureDevice != null
                        && _CaptureSource.State == CaptureState.Started) {
                        _CaptureSource.Stop();

                        // Disconnect File Sink.
                        _FileSink.CaptureSource = null;
                        _FileSink.IsolatedStorageFileName = null;
                    }
                    _CaptureSource.Start();
                }
                catch (Exception) {
                    // TODO Log
                }

                _HasRecording.OnNext(true);

                OpenVideoStream(OpenRecording());

                UpdateUI(PlayState.Previewing,
                    record: true,
                    play: true,
                    pause: false
                    );
            }
            else if (_State == PlayState.Playing) {
                if (!_CanRecord) {
                    UpdateUI(PlayState.Paused,
                        record: false,
                        play: true,
                        pause: false
                        );
                }
                else {
                    UpdateUI(PlayState.Previewing,
                    record: true,
                    play: true,
                    pause: false
                    );
                }

                if (_Player.CurrentState != MediaElementState.Closed) {
                    // If playback ended, we need to start it again.
                    // Otherwise the call to Stop() will only work some times
                    // and the player might hang at the last frame
                    if (_Player.CurrentState == MediaElementState.Paused) {
                        _Player.Play();
                    }
                    _Player.Stop();
                }
            }
            else {

            }
        }

        void OpenVideoStream(Stream video) {
            if (_OpenedVideo != video) {
                _Player.SetSource(video);
                _OpenedVideo = video;
            }
        }

        void CloseVideoStream() {
            if (_OpenedVideo != null) {
                _Player.Source = null;
                _OpenedVideo.Dispose();
                _OpenedVideo = null;
            }
        }

        private void UpdateUI(PlayState newState, bool record, bool play, bool pause) {
            _Record.IsExecutable = record;
            _Play.IsExecutable = play;
            _Stop.IsExecutable = pause;

            var playerStates = new[] { PlayState.Paused, PlayState.Playing };

            if (newState != _State) {
                var currentlyInPlayer = playerStates.Contains(_State);
                var willBeInPlayer = playerStates.Contains(newState);

                if (currentlyInPlayer != willBeInPlayer) {
                    if (currentlyInPlayer) {
                        PlayerToViewfinder();
                    }
                    else {
                        ViewfinderToPlayer();
                    }
                }
            }
            _State = newState;
        }

        private void ViewfinderToPlayer() {
            try {
                if (_CaptureSource.State == CaptureState.Started) {
                    _CaptureSource.Stop();
                }
                _ViewFinder.Fill = null;

                _ViewFinder.Visibility = System.Windows.Visibility.Collapsed;

                _Player.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception) {
                // TODO
                Debugger.Break();
            }
        }

        private void PlayerToViewfinder() {
            try {
                _Player.Visibility = System.Windows.Visibility.Collapsed;

                // Display the video on the viewfinder.
                if (_CaptureSource.VideoCaptureDevice != null) {
                    if (_CaptureSource.State == CaptureState.Started) {
                        _CaptureSource.Stop();
                    }

                    _Brush.SetSource(_CaptureSource);
                    _ViewFinder.Fill = _Brush;
                    _ViewFinder.Visibility = System.Windows.Visibility.Visible;

                    _CaptureSource.Start();
                }
            }
            catch (Exception) {
            }
        }



        private string NewVideoFilePath() {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!iso.DirectoryExists("Temp"))
                {
                    iso.CreateDirectory("Temp");
                }
            }

            return string.Format("Temp/{0}.mp4", Guid.NewGuid());
        }

        private Stream OpenRecording() {
            var iso = IsolatedStorageFile.GetUserStoreForApplication();

            if (iso.FileExists(_RecordingFileName)) {
                return new IsolatedStorageFileStream(
                    _RecordingFileName,
                    FileMode.Open,
                    FileAccess.Read,
                    iso
                    );
            }
            else {
                iso.Dispose();
                return Stream.Null;
            }
        }

        public void Dispose() {
            if (_Brush != null) {
                _Brush = null;
            }
            if (_ViewFinder != null) {
                _ViewFinder = null;
            }
            if (_Player != null) {
                _Player.Source = null;
                _Player.MediaEnded -= MediaEnded;
            }
            if (_CaptureSource != null) {
                _CaptureSource.CaptureFailed -= CaptureFailed;
            }
            CloseVideoStream();
        }

        private void CaptureFailed(object sender, System.Windows.ExceptionRoutedEventArgs e) {
            // TODO Log
            if (Stop.CanExecute(null)) {
                Stop.Execute(null);
            }
        }

        private void MediaEnded(object sender, System.Windows.RoutedEventArgs e) {
            if (Stop.CanExecute(null)) {
                Stop.Execute(null);
            }
        }


    }
}
