using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels.Utility {
    public interface IUploadVM<T> {
        MultipleSelectionHelper<T> Items { get; }
        IObservable<Unit> Refresh();
        IObservable<Tuple<int, int>> Upload();
    }

    public class UploadVM : PageVMBase {
        public enum Pivots {
            data,
            multimedia
        }



        private readonly IFieldDataService Storage;
        private readonly IConnectivityService Connectivity;
        private readonly IDiversityServiceClient Service;
        private readonly IKeyMappingService Mapping;


        readonly IUploadVM<IElementVM> _FieldData;
        public MultipleSelectionHelper<IElementVM> FieldData { get { return _FieldData.Items; } }

        readonly IUploadVM<MultimediaObjectVM> _Multimedia;
        public MultipleSelectionHelper<MultimediaObjectVM> Multimedia { get { return _Multimedia.Items; } }



        private Pivots _CurrentPivot;

        public Pivots CurrentPivot {
            get {
                return _CurrentPivot;
            }
            set {
                this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value);
            }
        }

        public bool IsOnlineAvailable { get { return _IsOnlineAvailable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;

        public ReactiveCommand StartUpload { get; private set; }

        public ReactiveCommand CancelUpload { get; private set; }


        public bool IsUploading { get { return _IsUploading.Value; } }
        private ObservableAsPropertyHelper<bool> _IsUploading;





        public UploadVM(
            IUploadVM<MultimediaObjectVM> Multimedia,
            IUploadVM<IElementVM> FieldData,
            IFieldDataService Storage,
            INotificationService Notifications,
            IConnectivityService Connectivity,
            IDiversityServiceClient Service,
            IKeyMappingService Mapping,
            [Dispatcher] IScheduler Dispatcher
            ) {
            this.Storage = Storage;
            this.Connectivity = Connectivity;
            this.Service = Service;
            this.Mapping = Mapping;

            this._Multimedia = Multimedia;
            this._FieldData = FieldData;

            var pivotOnChangeAndActivation =
            this.OnActivation()
                .SelectMany(_ => this.WhenAny(x => x.CurrentPivot, x => x.Value)
                    .Distinct()
                );

            pivotOnChangeAndActivation
                .Select(pivot => {
                    if (pivot == Pivots.data)
                    {
                        return _FieldData.Refresh()
                            .DisplayProgress(Notifications, DiversityResources.Sync_Info_CollectingModifications);
                    }
                    else
                    {
                        return _Multimedia.Refresh()
                            .DisplayProgress(Notifications, DiversityResources.Sync_Info_CollectingMultimedia);
                    }
                })
                .SelectMany(refresh => refresh.TakeUntil(this.OnDeactivation()))
                .Subscribe();


            _IsOnlineAvailable = this.ObservableToProperty(Connectivity.WifiAvailable(), x => x.IsOnlineAvailable, false);


            CancelUpload = new ReactiveCommand();
            StartUpload = new ReactiveCommand(Connectivity.WifiAvailable());

            _IsUploading =
                StartUpload
                .Select(_ => {
                    if (CurrentPivot == Pivots.data)
                        return _FieldData.Upload();
                    else
                        return _Multimedia.Upload();
                })
                .Select(upload => upload
                    .ShowServiceErrorNotifications(Notifications)
                    .ShowErrorNotifications(Notifications)
                    .TakeUntil(CancelUpload))
                .Do(upload => {
                    IObservable<string> notificationStream;
                    if (CurrentPivot == Pivots.data)
                        notificationStream = upload.Select(progress => string.Format("{0} ({1}/{2})", DiversityResources.Sync_Info_UploadingElement, progress.Item1, progress.Item2));
                    else
                        notificationStream = upload.Select(progress => string.Format("{0} ({1}/{2})", DiversityResources.Sync_Info_UploadingMultimedia, progress.Item1, progress.Item2));

                    Notifications.showProgress(notificationStream);
                })
                .SelectMany(upload =>
                    upload.IgnoreElements()
                    .Select(_ => false)
                    .StartWith(true)
                    .Concat(Observable.Return(false))
                    )
                    .ToProperty(this, x => x.IsUploading, scheduler: Dispatcher);


            this.OnDeactivation()
                .Select(_ => EventMessage.Default)
                .ToMessage(Messenger, MessageContracts.INIT);
        }




    }
}
