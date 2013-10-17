using DiversityPhone.Interface;
using DiversityPhone.Services;
using Microsoft;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;

namespace DiversityPhone.ViewModels {
    public class ImportExportVM : PageVMBase {
        public enum Pivot {
            local,
            remote
        }
        public enum Status {
            TakeAppData,
            TakeExternalImages,
            Restore,
            Delete,
            Upload,
            Download
        }

        public ICommand TakeSnapshot { get { return _TakeSnapshot; } }
        public ICommand DownloadSnapshot { get { return _DownloadSnapshot; } }
        public ICommand UploadSnapshot { get { return _UploadSnapshot; } }
        public ICommand DeleteSnapshot { get { return _DeleteSnapshot; } }
        public ICommand RestoreSnapshot { get { return _RestoreSnapshot; } }
        public ICommand RefreshRemote { get { return _RefreshRemote; } }
        public IListSelector<Snapshot> Snapshots { get; private set; }

        public IListSelector<string> RemoteSnapshots { get; private set; }

        private Status _CurrentStatus;
        public Status CurrentStatus {
            get { return _CurrentStatus; }
            set { this.RaiseAndSetIfChanged(x => x.CurrentStatus, ref _CurrentStatus, value); }
        }

        private int? _ProgressPercentage;
        public int? ProgressPercentage {
            get { return _ProgressPercentage; }
            set { this.RaiseAndSetIfChanged(x => x.ProgressPercentage, ref _ProgressPercentage, value); }
        }

        private Pivot _CurrentPivot;
        public Pivot CurrentPivot {
            get { return _CurrentPivot; }
            set { this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value); }
        }


        public bool IsBusy { get { return _IsBusy; } }

        private readonly IScheduler Dispatcher;

        private ReactiveCommand _TakeSnapshot, _DeleteSnapshot, _RestoreSnapshot, _UploadSnapshot, _DownloadSnapshot, _RefreshRemote;
        private ReactiveAsyncCommand _RefreshRemoteSnapshots, _RefreshSnapshots;
        private bool _IsBusy = false;
        private IProgress<int> _PercentageProgress;
        private IProgress<Tuple<BackupStage, int>> _BackupProgress;

        private void RaiseIsBusyChanged() {
            Dispatcher.Schedule(() => this.RaisePropertyChanged(x => x.IsBusy));
        }

        private void OperationFinished() {
            lock (this) {
                if (_IsBusy) {
                    _IsBusy = false;
                    RaiseIsBusyChanged();
                }
            }
        }

        /// <summary>
        /// Prevents more than one Operation from Running at the same time. 
        /// Tries to acquire the right to run a new Operation
        /// </summary>
        /// <returns>true if the new operation is allowed to proceed, false if there is already an operation running</returns>
        private bool TryStartOperation() {
            lock (this) {
                if (!_IsBusy) {
                    _IsBusy = true;

                    RaiseIsBusyChanged();
                    return true;
                }
                return false;
            }
        }

        private Status BackupStageToStatus(BackupStage stage) {
            switch (stage) {
                case BackupStage.AppData:
                    return Status.TakeAppData;
                case BackupStage.ExternalData:
                    return Status.TakeExternalImages;
                default:
                    throw new NotImplementedException();
            }
        }



        private string GetErrorStringForState() {
            string failureNotification = string.Empty;
            switch (CurrentStatus) {
                case Status.TakeAppData:
                case Status.TakeExternalImages:
                    failureNotification = DiversityResources.ImportExport_Failed_Take;
                    break;
                case Status.Restore:
                    failureNotification = DiversityResources.ImportExport_Failed_Restore;
                    break;
                case Status.Delete:
                    failureNotification = DiversityResources.ImportExport_Failed_Delete;
                    break;
                case Status.Upload:
                    failureNotification = DiversityResources.ImportExport_Failed_Upload;
                    break;
                case Status.Download:
                    failureNotification = DiversityResources.ImportExport_Failed_Download;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return failureNotification;
        }

        private void ShowErrorForState() {
            var errorString = GetErrorStringForState();
            Notifications.showNotification(errorString);
        }

        public ImportExportVM(
            ICurrentProfile Profile,
            IBackupService Backup,
            INotificationService Notifications,
            ICloudStorageService Cloud,
            [Dispatcher] IScheduler Dispatcher
            ) {
            Contract.Requires(Profile != null);
            Contract.Requires(Backup != null);
            Contract.Requires(Notifications != null);
            Contract.Requires(Dispatcher != null);

            this.Dispatcher = Dispatcher;

            Snapshots = new ListSelectionHelper<Snapshot>(Dispatcher);
            RemoteSnapshots = new ListSelectionHelper<string>(Dispatcher);
            _RefreshSnapshots = new ReactiveAsyncCommand();
            _RefreshRemoteSnapshots = new ReactiveAsyncCommand();

            _PercentageProgress = new Progress<int>(p => ProgressPercentage = p);
            _BackupProgress = new Progress<Tuple<BackupStage, int>>(t => {
                CurrentStatus = BackupStageToStatus(t.Item1);
                _PercentageProgress.Report(t.Item2);
            });

            var snapshotSelected = Snapshots
                .SelectedItemObservable
                .Select(s => s != null)
                .StartWith(false)
                .Publish()
                .RefCount();

            this.OnActivation()
                .SubscribeCommand(_RefreshSnapshots);

            Cloud.IsConnectedObservable()
                .CombineLatest(ActivationObservable, (connected, active) => connected & active)
                .Where(x => x)
                .SubscribeCommand(_RefreshRemoteSnapshots);


            _TakeSnapshot = new ReactiveCommand();
            AddStartErrorHandlingAndCompletion(
                _TakeSnapshot,
                _ => Backup.TakeSnapshot(_BackupProgress).ToObservable()
                )
                .SubscribeCommand(_RefreshSnapshots);

            _DeleteSnapshot = new ReactiveCommand(snapshotSelected);
            AddStartErrorHandlingAndCompletion(
            Snapshots.SelectedItemObservable
                .SampleMostRecent(_DeleteSnapshot),
                snap => StartDelete(Backup, snap)
                )
                .SubscribeCommand(_RefreshSnapshots);

            _RestoreSnapshot = new ReactiveCommand(snapshotSelected);
            AddStartErrorHandlingAndCompletion(
            Snapshots.SelectedItemObservable
                .SampleMostRecent(_RestoreSnapshot),
                snap => StartRestore(Backup, snap)
                )
                .Subscribe();

            var snapshotSelectedAndOnline = Observable.CombineLatest(snapshotSelected, Cloud.IsConnectedObservable(), (a, b) => a & b);

            _UploadSnapshot = new ReactiveCommand(snapshotSelectedAndOnline);
            AddStartErrorHandlingAndCompletion(
            Snapshots.SelectedItemObservable
                .SampleMostRecent(_UploadSnapshot),
                snap => StartUpload(Cloud, snap)
                )
                .ObserveOn(Dispatcher)
                .Do(_ => CurrentPivot = Pivot.remote)
                .SubscribeCommand(_RefreshRemoteSnapshots);

            _RefreshRemote = new ReactiveCommand(Cloud.IsConnectedObservable());
            _RefreshRemote
                .SubscribeCommand(_RefreshRemoteSnapshots);

            var remoteSnapshotSelectedAndOnline = Observable.CombineLatest(
                RemoteSnapshots.SelectedItemObservable.Select(r => r != null),
                Cloud.IsConnectedObservable(), (a, b) => a & b);

            _DownloadSnapshot = new ReactiveCommand(remoteSnapshotSelectedAndOnline);
            AddStartErrorHandlingAndCompletion(
                RemoteSnapshots.SelectedItemObservable
                .SampleMostRecent(_DownloadSnapshot),
                remote => StartDownload(Cloud, remote)
                    )
                    .ObserveOn(Dispatcher)
                    .Do(_ => CurrentPivot = Pivot.local)
                    .SubscribeCommand(_RefreshSnapshots);


            _RefreshSnapshots
                .RegisterAsyncObservable(_ =>
                    Backup.EnumerateSnapshots().ToList()
                    .Select(l => (from snap in l
                                  orderby snap.UserName ascending, snap.ProjectName ascending, snap.TimeTaken descending
                                  select snap).ToList() as IList<Snapshot>)
                    )
                .Subscribe(Snapshots.ItemsObserver);

            _RefreshRemoteSnapshots
                .RegisterAsyncTask(_ => Cloud.GetRemoteFolders())
                .Select(folders => folders.ToList() as IList<string>)
                .Subscribe(RemoteSnapshots.ItemsObserver);

            _RefreshRemoteSnapshots
                .AsyncStartedNotification
                .Select(_ =>
                    ObservableMixin.ReturnAndNever(DiversityResources.ImportExport_Info_QueryingRemote)
                    .TakeUntil(_RefreshRemoteSnapshots.AsyncCompletedNotification)
                    ).Subscribe(Notifications.showProgress);

            _RefreshRemoteSnapshots
                .ThrownExceptions
                .Subscribe(ex => { /*TODO Log*/ }); // Ignore Exceptions

        }

        private IObservable<Unit> AddStartErrorHandlingAndCompletion<T, TResult>(IObservable<T> observable, Func<T, IObservable<TResult>> operation) {
            // Handle any error by displaying a notification
            Func<Exception, IObservable<TResult>> handler = (ex) => {
                ShowErrorForState();
                return Observable.Empty<TResult>();
            };

            return observable
                .Where(_ => TryStartOperation()) //Aquire "IsBusy"
                .SelectMany(t => operation(t)
                    .IgnoreElements()
                    .Catch(handler)
                    .Select(_ => Unit.Default)
                    .Finally(OperationFinished) //Release "IsBusy" when done
                    .Concat(Observable.Return(Unit.Default)) // signal operation finished via output Observable
                    );


        }

        private IObservable<Unit> StartDownload(ICloudStorageService Cloud, string snap) {
            CurrentStatus = Status.Download;
            return Cloud.DownloadFolderAsync(snap, BackupService.SNAPSHOTS_DIRECTORY, _PercentageProgress).ToObservable();
        }

        private IObservable<Unit> StartUpload(ICloudStorageService Cloud, Snapshot snap) {
            CurrentStatus = Status.Upload;
            return Cloud.UploadFolderAsync(snap.FolderPath, _PercentageProgress).ToObservable();
        }

        private IObservable<Unit> StartRestore(IBackupService Backup, Snapshot snap) {
            CurrentStatus = Status.Restore;
            ProgressPercentage = null;
            return Backup.RestoreSnapshot(snap.FolderPath, _PercentageProgress).ToObservable();
        }

        private IObservable<Unit> StartDelete(IBackupService Backup, Snapshot snap) {
            CurrentStatus = Status.Delete;
            ProgressPercentage = null;
            return Backup.DeleteSnapshot(snap.FolderPath).ToObservable();
        }
    }
}
