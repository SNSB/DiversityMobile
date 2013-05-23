using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Threading;

namespace DiversityPhone.ViewModels.Utility
{
    public class MultimediaUploadVM : IUploadVM<MultimediaObjectVM>
    {
        readonly IDiversityServiceClient Service;
        readonly IFieldDataService Storage;
        readonly INotificationService Notifications;
        readonly IScheduler ThreadPool;
        readonly IKeyMappingService Mapping;
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
                                    foreach (var mmo in mmos)
                                    {
                                        uploadMultimedia(mmo)
                                            .LastOrDefault();

                                        Items.Remove(mmo);
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
                    byte[] data;
                    using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        var file = iso.OpenFile(mmo.Uri, System.IO.FileMode.Open);
                        data = new byte[file.Length];
                        file.Read(data, 0, data.Length);
                    }

                    return new { MMO = mmo, Data = data };
                })
                .SelectMany(t =>
                {
                    var upload =
                    Service.UploadMultimedia(t.MMO, t.Data)
                    .Do(uri => Storage.update(t.MMO, o => o.CollectionURI = uri))
                    .Select(_ => t.MMO)
                    .SelectMany(mmo => Service.InsertMultimediaObject(mmo)
                                              .Do(_ => Storage.MarkUploaded(mmo))
                    )
                    .DisplayProgress(Notifications, DiversityResources.Sync_Info_UploadingMultimedia)
                    .Publish();
                    upload.Connect();
                    return upload;
                });
        }
    }
}
