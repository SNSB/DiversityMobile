namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;


    public class MultimediaUploadVM : IUploadVM<MultimediaObjectVM>
    {
        readonly IDiversityServiceClient Service;
        readonly IFieldDataService Storage;
        readonly INotificationService Notifications;
        readonly IScheduler ThreadPool;
        readonly IKeyMappingService Mapping;
        readonly IStoreMultimedia Multimedia;
        readonly Func<MultimediaObject, MultimediaObjectVM> MultimediaVMFactory;

        public MultipleSelectionHelper<MultimediaObjectVM> Items { get; private set; }

        public IObservable<Unit> Refresh()
        {
            return Observable.Create<Unit>(obs =>
            {
                var cancelSource = new CancellationTokenSource();
                var cancel = cancelSource.Token;

                Items.OnNext(
                    Storage.getModifiedMMOs()
                    .Where(mmo => Mapping.ResolveToServerKey(mmo.OwnerType, mmo.RelatedId).HasValue)
                    .Select(mmo => MultimediaVMFactory(mmo))
                    .ToObservable(ThreadPool)
                    .TakeWhile(_ => !cancel.IsCancellationRequested)
                    .Finally(obs.OnCompleted));

                return Disposable.Create(cancelSource.Cancel);
            });
        }

        public IObservable<Tuple<int, int>> Upload()
        {
            return Observable.Defer(() => Items.SelectedItems)
                        .SelectMany(mmos => Observable.Create<Tuple<int, int>>(obs =>
                            {
                                var totalCount = mmos.Count();
                                var cancelSource = new CancellationTokenSource();
                                Observable.Start(() =>
                                {
                                    var cancel = cancelSource.Token;
                                    int idx = 0;
                                    obs.OnNext(Tuple.Create(idx, totalCount));
                                    foreach (var mmo in mmos)
                                    {
                                        try
                                        {
                                            uploadMultimedia(mmo)
                                                .LastOrDefault();

                                            Items.Remove(mmo);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (ex is ServiceNotAvailableException || ex is ServiceOperationException)
                                            {
                                                // Ignore Service Exceptions
                                                Notifications.showPopup(
                                                    string.Format("{0} {1}",
                                                    DiversityResources.Sync_Error_Multimedia,
                                                    ex.Message)
                                                );
                                            }
                                            else
                                            {
                                                throw;
                                            }
                                        }

                                        if (cancel.IsCancellationRequested) return;

                                        obs.OnNext(Tuple.Create(++idx, totalCount));
                                    }
                                }).Subscribe(_2 => { }, obs.OnError, obs.OnCompleted);
                                return Disposable.Create(cancelSource.Cancel);
                            })).Publish().RefCount();
        }

        public MultimediaUploadVM(
            IFieldDataService Storage,
            IKeyMappingService Mapping,
            IConnectivityService Connectivity,
            INotificationService Notifications,
            IDiversityServiceClient Service,
            IMessageBus Messenger,
            IStoreMultimedia MultimediaStore,
            [Dispatcher] IScheduler Dispatcher,
            [ThreadPool] IScheduler ThreadPool,
            Func<MultimediaObject, MultimediaObjectVM> MultimediaVMFactory
            )
        {
            this.Service = Service;
            this.Storage = Storage;
            this.Notifications = Notifications;
            this.ThreadPool = ThreadPool;
            this.Mapping = Mapping;
            this.Multimedia = MultimediaStore;
            this.MultimediaVMFactory = MultimediaVMFactory;

            Items = new MultipleSelectionHelper<MultimediaObjectVM>(Dispatcher);
        }

        private IObservable<Unit> uploadMultimedia(MultimediaObjectVM vm)
        {
            return Observable.Return(vm)
                .Select(v => v.Model)
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Select(mmo =>
                {
                    if (mmo.CollectionURI == null)
                    {
                        Stream file = null;
                        try
                        {
                            file = Multimedia.GetMultimedia(mmo.Uri);
                            if (file.Length <= 0)
                            {
                                file.Dispose();
                                return Observable.Empty<MultimediaObject>();
                            }
                        }
                        catch
                        {
                            if (file != null)
                            {
                                file.Dispose();
                            }
                            throw;
                        }
                        return Service.UploadMultimedia(mmo, file)
                            .Do(uri => Storage.update(mmo, o => o.CollectionURI = uri))
                            .Select(_ => mmo);
                    }
                    else
                    {
                        return Observable.Return(mmo);
                    }
                })
                .SelectMany(obs =>
                    obs.SelectMany(mmo => Service.InsertMultimediaObject(mmo).Select(_ => mmo))
                        .Do(mmo => Storage.MarkUploaded(mmo))
                        .Select(_ => Unit.Default)
                        .Publish()
                        .PermaRef()
                );
        }
    }
}
