namespace DiversityPhone.ViewModels {
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.IO;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Windows.Media.Imaging;

    public class ViewImageVM : ViewPageVMBase<MultimediaObject>, IDeletePageVM {
        readonly IStoreImages ImageStore;

        private Tuple<Stream, BitmapImage> _CurrentImage;

        private void SetCurrentImage(Tuple<Stream, BitmapImage> value) {
            if (_CurrentImage != value) {
                CleanupCurrentImage();
                _CurrentImage = value;
                this.RaisePropertyChanged(x => x.CurrentImage);
            }
        }

        private void CleanupCurrentImage() {
            var streamAndImage = _CurrentImage;
            if (streamAndImage != null) {
                if (streamAndImage.Item2 != null) {
                    streamAndImage.Item2.UriSource = null;
                }
                if (streamAndImage.Item1 != null) {
                    streamAndImage.Item1.Dispose();
                }
                _CurrentImage = null;
            }
        }

        public BitmapImage CurrentImage {
            get {
                return (_CurrentImage != null) ? _CurrentImage.Item2 : null;
            }
        }

        public IReactiveCommand Delete { get; private set; }


        public ViewImageVM(IStoreImages ImageStore,
            [Dispatcher] IScheduler Dispatcher)
            : base(mmo => mmo.MediaType == MediaType.Image) {
            this.ImageStore = ImageStore;

            Messenger
                .Listen<IElementVM<MultimediaObject>>(MessageContracts.VIEW)
                .Where(vm => vm.Model.MediaType == MediaType.Image && !vm.Model.IsNew())
                .Subscribe(x => Current = x);


            //View Old image
            CurrentModelObservable
                .ObserveOn(Dispatcher)
                .Where(mmo => !mmo.IsNew())
                .Select(mmo => {
                    var img = new BitmapImage();
                    Stream stream = null;
                    try {
                        stream = ImageStore.GetMultimedia(mmo.Uri);
                        img.SetSource(stream);
                        return Tuple.Create(stream, img);
                    }
                    catch (Exception) {
                        if (stream != null) {
                            stream.Dispose();
                        }
                        return null;
                    }
                })
                .Subscribe(SetCurrentImage);

            Delete = new ReactiveCommand();

            CurrentObservable
                .SampleMostRecent(Delete)
                .SelectMany(toBeDeleted => Notifications.showDecision(DiversityResources.Message_ConfirmDelete)
                    .Where(x => x)
                    .Select(_ => toBeDeleted)
                )
                .ObserveOn(Dispatcher)
                .Do(_ => SetCurrentImage(null))
                .ToMessage(Messenger, MessageContracts.DELETE);

            this.OnDeactivation()
                .Subscribe(_ => CleanupCurrentImage());
        }


    }
}
