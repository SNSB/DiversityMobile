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
using System.Threading.Tasks;
using DiversityPhone.Services.BackgroundTasks;

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


        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        private CancellationTokenSource search = null;
        
        public ReactiveCollection<ModifiedEventVM> SyncUnits { get; private set; }
        public ReactiveCollection<MultimediaVM> Multimedia { get; private set; }

        public ReactiveCommand UploadUnit { get; private set; }

        public ReactiveCommand UploadAll { get; private set; }

        private ReactiveAsyncCommand collectModifications = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand collectMultimedia = new ReactiveAsyncCommand();

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
            public object Image { get; private set; }

            public object Owner { get; private set; }           

            public MultimediaVM(MultimediaObject obj, object owner)
                :base(obj)
            {
                Owner = owner;

                if (obj.MediaType == MediaType.Image)
                {
                    Image =  new ImageVM(obj);
                }
            }
        }

        public SyncVM(Container ioc)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            SyncUnits = new ReactiveCollection<ModifiedEventVM>();
            this.OnActivation()
                .Select(_ => null as object)
                .Do(_ => search = new CancellationTokenSource())
                .Do(_ => SyncUnits.Clear())
                .Subscribe(collectModifications.Execute);

            this.OnDeactivation()
                .Subscribe(_ =>
                    {
                        if (search != null)
                        {
                            search.Cancel();
                            search = null;
                        }
                    });
            collectModifications.RegisterAsyncObservable(_ =>
                {
                    var res = new ReplaySubject<ModifiedEventVM>();
                    new Task(() => collectModificationsImpl(res, search.Token)).Start();
                    return res;
                })
               .Subscribe(ev => SyncUnits.Add(ev));
            Multimedia = new ReactiveCollection<MultimediaVM>();
            //Needs to be cleared?
            this.ObservableForProperty(x => x.CurrentPivot)
                .Value()
                .Where(p => p == Pivots.multimedia)
                //.Take(1)
                .Subscribe(_ => collectMultimedia.Execute(null));
            collectMultimedia.RegisterAsyncObservable(_ =>
                {
                    var res = new ReplaySubject<MultimediaVM>();
                    new Task(() => collectMultimediaImpl(res, search.Token)).Start();
                    return res;
                })
                .Subscribe(mvm => Multimedia.Add(mvm));

           
            _IsBusy = this.ObservableToProperty(collectModifications.ItemsInflight.Select(i => i > 0),x => x.IsBusy, false);

            var backg = ioc.Resolve<IBackgroundService>();
            var uploadTask = backg.getTaskObject<UploadEventTask>();
            var multimediaUpload = backg.getTaskObject<UploadMultimediaTask>();
            var uploading = uploadTask                
                                .BusyObservable
                                .BooleanOr(multimediaUpload.BusyObservable)
                                .Select(x => !x)
                                .StartWith(false);           
            var canUpload = uploading.Select(x => !x);
                
            uploadTask.AsyncCompletedNotification
                .Select(arg => arg as Event)
                .Where(arg => arg != null)
                .SubscribeOnDispatcher()
                .Subscribe(unit =>
                    {
                        var vm = SyncUnits.Where(v => v.Event.Model == unit).FirstOrDefault();
                        if (vm != null)
                            SyncUnits.Remove(vm);
                    });

            multimediaUpload.AsyncCompletedNotification
                .Select(arg => arg as MultimediaObject)
                .Where(arg => arg != null)
                .Subscribe(mmo =>
                    {
                        var vm = Multimedia.Where(v => v.Model == mmo).FirstOrDefault();
                        if(vm != null)
                            Multimedia.Remove(vm);
                    });


            UploadUnit = new ReactiveCommand(canUpload);
            UploadUnit
                .Select(unit => unit as ModifiedEventVM)
                .Where(unit => unit != null)
                .Subscribe(unit => backg.startTask<UploadEventTask>(unit.Event.Model));
            UploadUnit
                .Select(unit => unit as MultimediaVM)
                .Where(unit => unit != null)
                .Subscribe(unit => backg.startTask<UploadMultimediaTask>(unit.Model));

            UploadAll = new ReactiveCommand(canUpload);
            UploadAll
                .Where(_ => CurrentPivot == Pivots.data)                
                .Subscribe(_ =>
                    {
                        var syncUnits = new List<ModifiedEventVM>(SyncUnits).ToObservable();
                        uploadTask.AsyncCompletedNotification
                            .StartWith(new object[]{null})
                            .Zip(syncUnits, (_2,unit) => unit)                            
                            .Subscribe(UploadUnit.Execute);                        
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
                                uploadAllMultimedia(multimediaUpload); 
                        }))
               );
               
                
        }

        private struct SyncUnitIncrement
        {
            public int? Unit { get; set; }
            public int Increment { get; set; }
        }

        private void collectModificationsImpl(ISubject<ModifiedEventVM> outputSubject, CancellationToken cancellation)
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

                    outputSubject.OnNext(new ModifiedEventVM(ev,series));
                }                    
            }            
        }

        private void collectMultimediaImpl(ISubject<MultimediaVM> outputSubject, CancellationToken cancellation)
        {
            var mmos = Storage.getMultimediaObjectsForUpload();

            foreach (var mmo in mmos)
            {
                if (!cancellation.IsCancellationRequested)
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

                    outputSubject.OnNext(new MultimediaVM(mmo, ownerVM));
                }
            }

        }

        private void uploadAllMultimedia(UploadMultimediaTask task)
        {
            var multimedia = new List<MultimediaVM>(Multimedia).ToObservable();
                           
            task.AsyncCompletedNotification
                .StartWith(new object[]{null})
                .Zip(multimedia, (_, multvm) => multvm)                
                .Subscribe(UploadUnit.Execute);
        }
    }
}
