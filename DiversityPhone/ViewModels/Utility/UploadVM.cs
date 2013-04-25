using System;

using ReactiveUI;
using ReactiveUI.Xaml;
using System.Collections.Generic;
using DiversityPhone.Services;
using System.Linq;
using DiversityPhone.Model;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive;
using DiversityPhone.Interface;

namespace DiversityPhone.ViewModels.Utility
{
    public class UploadVM : PageVMBase
    {
        public enum Pivots
        {
            data,
            multimedia
        }

        public enum SyncLevel
        {
            All,
            EventSeries,
            Event,
            Specimen,
            IdentificationUnit
        }

        private readonly IFieldDataService Storage;
        private readonly INotificationService Notifications;
        private readonly IConnectivityService Connectivity;
        private readonly IDiversityServiceClient Service;
        private readonly IKeyMappingService Mapping;



        private Pivots _CurrentPivot;

        public Pivots CurrentPivot
        {
            get
            {
                return _CurrentPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value);
            }
        }



        public bool IsOnlineAvailable { get { return _IsOnlineAvailable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;






        public ListSelectionHelper<SyncLevel> SyncLevels { get; private set; }

        public ReactiveCollection<IElementVM> SyncUnits { get; private set; }
        public ReactiveCollection<MultimediaObjectVM> Multimedia { get; private set; }

        public ReactiveCommand<IElementVM> UploadTree { get; private set; }

        public ReactiveCommand<MultimediaObjectVM> UploadMultimedia { get; private set; }

        public ReactiveCommand UploadAll { get; private set; }

        private ISubject<SyncLevel?> recollectModifications = new Subject<SyncLevel?>();

        private long _UploadsInProgress = 0;
        private IDisposable _CurrentUpload = Disposable.Empty;

        private bool TryStartUpload()
        {
            bool aquired = false;
            lock (this)
            {
                aquired = _UploadsInProgress == 0;
                _UploadsInProgress = 1;
            }
            if (aquired)
                Observable.Start(() => this.RaisePropertyChanged(x => x.IsUploading), DispatcherScheduler.Current);
            return aquired;
        }
        private void UploadCompleted()
        {
            if (!IsUploading)
                throw new InvalidOperationException("No running upload to be completed");

            _UploadsInProgress = 0;

            Observable.Start(() => this.RaisePropertyChanged(x => x.IsUploading), DispatcherScheduler.Current);
        }
        public bool IsUploading
        {
            get { return _UploadsInProgress > 0; }
        }

        public ReactiveCommand CancelUpload { get; private set; }

        public UploadVM(
             IFieldDataService Storage,
            INotificationService Notifications,
            IConnectivityService Connectivity,
            IDiversityServiceClient Service,
            IKeyMappingService Mapping
            )
        {
            this.Storage = Storage;
            this.Notifications = Notifications;
            this.Connectivity = Connectivity;
            this.Service = Service;
            this.Mapping = Mapping;

            SyncLevels = new ListSelectionHelper<SyncLevel>();
            SyncLevels.Items = new List<SyncLevel>()
            {
                SyncLevel.All,
                SyncLevel.EventSeries,
                SyncLevel.Event,
                SyncLevel.Specimen,
                SyncLevel.IdentificationUnit
            };

            SyncUnits = new ReactiveCollection<IElementVM>();

            this.ActivationObservable
                .CombineLatest(SyncLevels, (a, l) => (a) ? l : null as SyncLevel?)
                .Subscribe(recollectModifications);
            recollectModifications
                .Do(_ => SyncUnits.Clear())
                .Where(l => l.HasValue)
                .SelectMany(level =>
                    {
                        return
                        collectModificationsImpl(level.Value)
                        .ToObservable(ThreadPoolScheduler.Instance)
                        .TakeUntil(recollectModifications)
                        .TakeUntil(UploadAll)
                        .DisplayProgress(Notifications, DiversityResources.Sync_Info_CollectingModifications);
                    })
                .ObserveOnDispatcher()
                .Subscribe(SyncUnits.Add);

            SyncLevels.SelectedItem = SyncLevel.All;

            Multimedia = new ReactiveCollection<MultimediaObjectVM>();
            //Needs to be cleared?
            this.ObservableForProperty(x => x.CurrentPivot)
                .Value()
                .Select(p => p == Pivots.multimedia)
                .Do(_ => Multimedia.Clear())
                .Where(onMultimedia => onMultimedia)
                .SelectMany(_ =>
                    {
                        return
                        Storage.getModifiedMMOs()
                        .Where(mmo => Mapping.ResolveToServerKey(mmo.OwnerType, mmo.RelatedId).HasValue)
                        .Select(mmo => new MultimediaObjectVM(mmo))
                        .ToObservable(ThreadPoolScheduler.Instance)
                        .TakeUntil(this.OnDeactivation())
                        .DisplayProgress(Notifications, DiversityResources.Sync_Info_CollectingMultimedia);
                    })
                .ObserveOnDispatcher()
                .Subscribe(Multimedia.Add);





            _IsOnlineAvailable = this.ObservableToProperty(Connectivity.WifiAvailable(), x => x.IsOnlineAvailable, false);



            UploadTree = new ReactiveCommand<IElementVM>(Connectivity.WifiAvailable());
            UploadTree
                .Where(_ => TryStartUpload())
                .Subscribe(vm =>
                {
                    _CurrentUpload = uploadTree(vm)
                        .Finally(() => UploadCompleted())
                        .ObserveOnDispatcher()
                        .Subscribe(_ => { }, () => SyncUnits.Remove(vm));
                });


            UploadMultimedia = new ReactiveCommand<MultimediaObjectVM>(Connectivity.WifiAvailable());
            UploadMultimedia
                .Where(_ => TryStartUpload())
                .Subscribe(vm =>
                    {
                        _CurrentUpload = uploadMultimedia(vm)
                            .Finally(() => UploadCompleted())
                            .HandleServiceErrors(Notifications, Messenger, Observable.Empty<Unit>())
                            .ObserveOnDispatcher()
                            .Subscribe(_ => Multimedia.Remove(vm));
                    });



            UploadAll = new ReactiveCommand(Connectivity.WifiAvailable());
            UploadAll
                .Where(_ => TryStartUpload())
                .Subscribe(_ =>
                    {
                        if (CurrentPivot == Pivots.data)
                        {


                            _CurrentUpload = Observable.Start(() =>
                                {
                                    var data = collectModificationsImpl(SyncLevel.All);
                                    foreach (var vm in data)
                                    {
                                        uploadTree(vm)
                                            .HandleServiceErrors(Notifications, Messenger, Observable.Empty<Unit>())
                                            .Wait();
                                    }
                                })
                                .Finally(() => UploadCompleted())
                                .HandleServiceErrors(Notifications, Messenger, Observable.Empty<Unit>())
                                .ObserveOnDispatcher()
                                .Subscribe(_2 => { }, () => recollectModifications.OnNext(SyncLevels.SelectedItem));
                        }
                        else
                        {
                            var mmos = Multimedia.ToList();

                            _CurrentUpload = Observable.Create<MultimediaObjectVM>(obs =>
                                {
                                    bool cancel = false;
                                    Observable.Start(() =>
                                        {
                                            try
                                            {
                                                foreach (var mmo in mmos)
                                                {
                                                    uploadMultimedia(mmo)
                                                        .HandleServiceErrors(Notifications, Messenger, Observable.Empty<Unit>())
                                                        .LastOrDefault();
                                                    if (cancel) return;

                                                    obs.OnNext(mmo);
                                                }
                                                obs.OnCompleted();
                                            }
                                            catch (Exception ex)
                                            {
                                                obs.OnError(ex);
                                            }
                                        });

                                    return Disposable.Create(() => cancel = true);
                                })
                                .Finally(() => UploadCompleted())
                                .ObserveOnDispatcher()
                                .Subscribe(vm => Multimedia.Remove(vm));
                        }
                    });


            CancelUpload = new ReactiveCommand();
            CancelUpload
                .Subscribe(_ =>
                    {
                        var upl = _CurrentUpload;
                        if (upl != null)
                            upl.Dispose();
                    });

            this.OnDeactivation()
                .Select(_ => EventMessage.Default)
                .ToMessage(MessageContracts.INIT);
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
                        .Do(uri => Storage.update(t.MMO, o => o.CollectionUri = uri))
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

        private IObservable<Unit> uploadES(EventSeries es)
        {
            return Service.InsertEventSeries(es, Storage.getGeoPointsForSeries(es.SeriesID.Value).Select(gp => gp as ILocalizable))
                    .SelectMany(_ => Storage.getEventsForSeries(es).Select(ev => uploadEV(ev)))
                    .SelectMany(obs => obs);
        }

        private IObservable<Unit> uploadEV(Event ev)
        {
            return Service.InsertEvent(ev, Storage.getPropertiesForEvent(ev.EventID))
                    .SelectMany(_ => Storage.getSpecimenForEvent(ev).Select(s => uploadSpecimen(s)))
                    .SelectMany(x => x);
        }

        private IObservable<Unit> uploadSpecimen(Specimen s)
        {
            return Service.InsertSpecimen(s)
                .SelectMany(_ => Storage.getTopLevelIUForSpecimen(s.SpecimenID).Select(iu => uploadIU(iu)))
                .SelectMany(x => x);
        }

        private IObservable<Unit> uploadIU(IdentificationUnit iu)
        {
            return Service.InsertIdentificationUnit(iu, Storage.getIUANForIU(iu))
                .SelectMany(_ => Storage.getSubUnits(iu).Select(sub => uploadIU(sub)))
                .SelectMany(x => x);
        }
        private IObservable<Unit> uploadTree(IElementVM vm)
        {
            var model = vm.Model;
            IObservable<Unit> res = Observable.Empty<Unit>();

            if (model is EventSeries)
                res = uploadES(model as EventSeries);
            if (model is Event)
                res = uploadEV(model as Event);
            if (model is Specimen)
                res = uploadSpecimen(model as Specimen);
            if (model is IdentificationUnit)
                res = uploadIU(model as IdentificationUnit);

            return res
                .DisplayProgress(Notifications, DiversityResources.Sync_Info_Uploading);
        }

        private IEnumerable<IElementVM> collectModificationsImpl(SyncLevel synclevel)
        {
            if (synclevel == SyncLevel.All)
            {
                var stream = collectModificationsImpl(SyncLevel.EventSeries)
                    .Concat(collectModificationsImpl(SyncLevel.Event))
                    .Concat(collectModificationsImpl(SyncLevel.Specimen))
                    .Concat(collectModificationsImpl(SyncLevel.IdentificationUnit));
                foreach (var vm in stream)
                    yield return vm;
            }
            else
            {
                IEnumerable<IElementVM> stream;
                using (var ctx = new DiversityDataContext())
                {

                    switch (synclevel)
                    {
                        case SyncLevel.EventSeries:
                            stream = from es in ctx.EventSeries
                                     where es.CollectionSeriesID == null && es.SeriesEnd != null
                                     select new EventSeriesVM(es) as IElementVM;
                            break;
                        case SyncLevel.Event:
                            stream = from es in ctx.EventSeries
                                     where es.CollectionSeriesID != null
                                     join ev in ctx.Events on es.SeriesID equals ev.SeriesID
                                     where ev.CollectionEventID == null
                                     select new EventVM(ev) as IElementVM;
                            stream = stream.Concat(from ev in ctx.Events
                                                   where ev.SeriesID == null && ev.CollectionEventID == null
                                                   select new EventVM(ev) as IElementVM);

                            break;
                        case SyncLevel.Specimen:
                            stream = from ev in ctx.Events
                                     where ev.CollectionEventID != null
                                     join s in ctx.Specimen on ev.EventID equals s.EventID
                                     where s.CollectionSpecimenID == null
                                     select new SpecimenVM(s) as IElementVM;
                            break;
                        case SyncLevel.IdentificationUnit:
                            stream = from iu in
                                         //New IU with parent Spec Uploaded
                                         (from s in ctx.Specimen
                                          where s.CollectionSpecimenID != null
                                          join iu in ctx.IdentificationUnits on s.SpecimenID equals iu.SpecimenID
                                          where iu.CollectionUnitID == null
                                          && iu.RelatedUnitID == null
                                          select iu)
                                             //New IU with parent Unit uploaded
                                     .Union(from u in ctx.IdentificationUnits
                                            where u.CollectionUnitID != null
                                            join sub in ctx.IdentificationUnits on u.UnitID equals sub.RelatedUnitID
                                            where sub.CollectionUnitID == null
                                            select sub)
                                     select new IdentificationUnitVM(iu) as IElementVM;
                            break;
                        default:
                            throw new ArgumentException("synclevel");
                    }
                    foreach (var vm in stream)
                        yield return vm;
                }
            }
        }
    }
}
