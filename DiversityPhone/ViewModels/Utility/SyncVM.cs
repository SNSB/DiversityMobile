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
    public class SyncVM : PageViewModel
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
        
        public ReactiveCollection<SyncUnitVM> SyncUnits { get; private set; }

        public ReactiveCommand UploadUnit { get; private set; }

        public ReactiveCommand UploadAll { get; private set; }

        private ReactiveAsyncCommand collectModifications = new ReactiveAsyncCommand();

        public class SyncUnitVM : ReactiveObject
        {
            public EventSeriesVM Series { get; private set; }

            public EventVM Event { get; private set; }

            public SyncUnitVM(Event ev, EventSeries series)
            {                
                Event = new EventVM( ev);
                Series = new EventSeriesVM(series);                
            }
        }

        

        public SyncVM(Container ioc)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            SyncUnits = new ReactiveCollection<SyncUnitVM>();
            StateObservable
                .Do(_ => search = new CancellationTokenSource())
                .Do(_ => SyncUnits.Clear())
                .Subscribe(collectModifications.Execute);
            collectModifications.RegisterAsyncObservable(_ => 
                {
                    var res = new ReplaySubject<Event>();
                    new Task(() => collectModificationsImpl(res, search.Token)).Start();
                    return res;
                })
                .Subscribe(ev => addModifiedEvent(ev));
            _IsBusy = this.ObservableToProperty(collectModifications.ItemsInflight.Select(i => i > 0),x => x.IsBusy, false);

            var backg = ioc.Resolve<IBackgroundService>();
            var uploadTask = backg.getTaskObject<UploadSeriesTask>();
            var uploading = uploadTask
                                .BusyObservable.Select(x => !x)
                                .StartWith(false);
            var collectingModifications = collectModifications
                .ItemsInflight
                .Select(items => items > 0)
                .StartWith(false);
            var canUpload = uploading.Select(x => !x);
                //.CombineLatest(collectingModifications, (up, coll) => !up && !coll);
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


            UploadUnit = new ReactiveCommand(canUpload);
            UploadUnit
                .Select(unit => unit as SyncUnitVM)
                .Where(unit => unit != null)
                .Subscribe(unit => backg.startTask<UploadSeriesTask>(unit.Event.Model));

            UploadAll = new ReactiveCommand(canUpload);
            UploadAll
                .Select(_ => SyncUnits.FirstOrDefault())
                .Where(vm => vm != null)
                .Subscribe(first =>
                    {
                        uploadTask.AsyncCompletedNotification
                            .Select(unit => SyncUnits.Where(v => v.Event.Model != unit).FirstOrDefault())
                            .TakeWhile(next => next != null)
                            .Subscribe(UploadUnit.Execute);

                        UploadUnit.Execute(first);
                    });
                
        }

        private struct SyncUnitIncrement
        {
            public int? Unit { get; set; }
            public int Increment { get; set; }
        }

        public override void SaveState()
        {
            if(search != null)
            {
                search.Cancel();
                search = null;
            }
        }

        private void collectModificationsImpl(ISubject<Event> outputSubject, CancellationToken cancellation)
        {
            var events = Storage.getAllEvents();

            foreach (var ev in events)
            {
                if (ev.IsModified())
                    outputSubject.OnNext(ev);
                else
                {
                    var modifications = Storage.getNewHierarchyToSyncBelow(ev);
                    if (modifications.Specimen.Any()
                        || modifications.Properties.Any()
                        || modifications.IdentificationUnits.Any()
                        || modifications.IdentificationUnitAnalyses.Any())
                        outputSubject.OnNext(ev);
                        

                }
            }
            
        }

        private void addModifiedEvent(Event ev)
        {         
            var series = Storage.getEventSeriesByID(ev.SeriesID);                 
                    
            SyncUnits.Add(new SyncUnitVM(ev,series));                
        }
    }
}
