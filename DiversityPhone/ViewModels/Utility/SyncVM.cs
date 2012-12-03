using System;
using Funq;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Collections.Generic;
using DiversityPhone.Services;
using System.Linq;
using DiversityPhone.Model;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Subjects;
using DiversityPhone.Services.BackgroundTasks;
using System.Reactive.Concurrency;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels.Utility
{
    public class SyncVM : PageVMBase
    {
        public enum Pivots
        {
            data,
            multimedia
        }


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
        

        private IFieldDataService Storage;
        private INotificationService Notifications;
        private IConnectivityService Connectivity;
        
        public ReactiveCollection<ModifiedEventVM> SyncUnits { get; private set; }
        public ReactiveCollection<MultimediaVM> Multimedia { get; private set; }

        public ReactiveCommand<ModifiedEventVM> UploadEvent { get; private set; }

        public ReactiveCommand<MultimediaVM> UploadMultimedia { get; private set; }

        public ReactiveCommand UploadAll { get; private set; }

        public class ModifiedEventVM 
        {
            public EventSeriesVM Series { get; private set; }

            public EventVM Event { get; private set; }

            public ModifiedEventVM(Event ev, EventSeries series)
            {                
                Event = new EventVM( ev);
                Series = new EventSeriesVM(series);                
            }
        }

        public class MultimediaVM : MultimediaObjectVM
        {
            public object Owner { get; private set; }           

            public MultimediaVM(MultimediaObject obj, object owner)
                : base(obj)
            {
                Owner = owner;
            }
        }

        public SyncVM(Container ioc)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            Notifications = ioc.Resolve<INotificationService>();
            Connectivity = ioc.Resolve<IConnectivityService>();

            SyncUnits = new ReactiveCollection<ModifiedEventVM>();
            this.OnActivation()                
                .Do(_ => SyncUnits.Clear())
                .SelectMany(_ => 
                    {
                        var notification = Notifications.showProgress(DiversityResources.Sync_Info_CollectingModifications);
                        return
                        collectModificationsImpl()
                        .ToObservable(ThreadPoolScheduler.Instance)
                        .TakeUntil(this.OnDeactivation())     
                        .Finally(notification.Dispose);
                    })
                .ObserveOnDispatcher()
                .Subscribe(SyncUnits.Add);

            Multimedia = new ReactiveCollection<MultimediaVM>();
            //Needs to be cleared?
            this.ObservableForProperty(x => x.CurrentPivot)
                .Value()
                .Where(p => p == Pivots.multimedia)
                .Do(_ => Multimedia.Clear())
                .SelectMany(_ => 
                    {
                        var notification = Notifications.showProgress(DiversityResources.Sync_Info_CollectingMultimedia);
                        return
                        enumerateModifiedMMOs()
                        .ToObservable(ThreadPoolScheduler.Instance)
                        .TakeUntil(this.OnDeactivation())
                        .Finally(notification.Dispose);
                    })
                .ObserveOnDispatcher()
                .Subscribe(Multimedia.Add);            

           
            

           
            
            
                     
            var canUpload = new BehaviorSubject<bool>(true);           


            UploadEvent = new ReactiveCommand<ModifiedEventVM>(canUpload);
            UploadEvent
                .CheckConnectivity(Connectivity, Notifications)
                .Do(_ => canUpload.OnNext(false))
                .Select(unit => new { Unit = unit, Task = UploadEventTask.Start(ioc, unit.Event.Model) })
                .Subscribe(t =>
                    t.Task
                    .ObserveOnDispatcher()
                    .HandleServiceErrors(Notifications, Messenger)
                    .Finally(() => canUpload.OnNext(true))
                    .Subscribe(_ => { }, ex => { }, () => SyncUnits.Remove(t.Unit))
                    );

            UploadMultimedia = new ReactiveCommand<MultimediaVM>(canUpload);
            UploadMultimedia
                .CheckConnectivity(Connectivity, Notifications)
                .Select(unit => new { Unit = unit, Task = UploadMultimediaTask.Start(ioc, unit.Model) })
                .Subscribe(t =>
                    t.Task
                    .ObserveOnDispatcher()
                    .HandleServiceErrors(Notifications, Messenger)
                    .Finally(() => canUpload.OnNext(true))
                    .Subscribe(_ => { }, ex => { }, () => Multimedia.Remove(t.Unit))
                    );

            UploadAll = new ReactiveCommand(canUpload);
            UploadAll
                .Where(_ => CurrentPivot == Pivots.data)                
                .Subscribe(_ =>
                    {
                        var syncUnits = new List<ModifiedEventVM>(SyncUnits).ToObservable();
                        canUpload.Where(can => can)
                            .Zip(syncUnits, (_2,unit) => unit)                            
                            .Subscribe(UploadEvent.Execute);                        
                    });
            Messenger.RegisterMessageSource(
            UploadAll
               .Where(_ => CurrentPivot == Pivots.multimedia)
               .Select(_ => 
                   new DialogMessage(Messages.DialogType.YesNo, 
                        DiversityResources.Sync_Dialog_UploadAll_Caption, 
                        DiversityResources.Sync_Dialog_UploadAll_Text, 
                        (res) => 
                        { 
                            if (res == DialogResult.OKYes) 
                                uploadAllMultimedia(canUpload); 
                        }))
               );

            this.OnDeactivation()
                .Select(_ => EventMessage.Default)
                .ToMessage(MessageContracts.INIT);
        }

        private struct SyncUnitIncrement
        {
            public int? Unit { get; set; }
            public int Increment { get; set; }
        }

        private IEnumerable<ModifiedEventVM> collectModificationsImpl()
        {
            var events = Storage.getAllEvents();

            foreach (var ev in events)
            {
                bool modified = ev.IsModified();
                if (!modified)
                {
                    var modifications = Storage.getNewHierarchyToSyncBelow(ev);
                    if (modifications.Specimen.Any()
                        || modifications.Properties.Any()
                        || modifications.IdentificationUnits.Any()
                        || modifications.IdentificationUnitAnalyses.Any())    
                        modified = true;
                }
                if(modified)
                {
                    var series = Storage.getEventSeriesByID(ev.SeriesID);

                    yield return new ModifiedEventVM(ev,series);
                }                    
            }            
        }

        private IEnumerable<MultimediaVM> enumerateModifiedMMOs()
        {
            var mmos = Storage.getMultimediaObjectsForUpload();

            foreach (var mmo in mmos)
            {                                  
                object ownerVM;
                switch (mmo.OwnerType)
                {                        
                    case ReferrerType.EventSeries:
                        ownerVM = new EventSeriesVM(Storage.getEventSeriesByID(mmo.RelatedId));
                        break;
                    case ReferrerType.Event:
                        ownerVM = new EventVM(Storage.getEventByID(mmo.RelatedId));
                        break;
                    case ReferrerType.Specimen:
                        ownerVM = new SpecimenVM(Storage.getSpecimenByID(mmo.RelatedId));
                        break;
                    case ReferrerType.IdentificationUnit:
                        ownerVM = new IdentificationUnitVM(Storage.getIdentificationUnitByID(mmo.RelatedId));
                        break;
                    default:
                        continue;
                }

                yield return new MultimediaVM(mmo, ownerVM);                
            }
        }

        private void uploadAllMultimedia(IObservable<bool> canUpload)
        {
            var multimedia = new List<MultimediaVM>(Multimedia).ToObservable();
                           
            canUpload.Where(can => can)
                .Zip(multimedia, (_, multvm) => multvm)                
                .Subscribe(UploadMultimedia.Execute);
        }
    }
}
