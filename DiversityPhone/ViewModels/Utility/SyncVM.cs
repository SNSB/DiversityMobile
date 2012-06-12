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
        private IFieldDataService Storage;


        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        private CancellationTokenSource search = null;
        
        public ReactiveCollection<SyncUnitVM> SyncUnits { get; private set; }

        public ReactiveCommand UploadUnit { get; private set; }

        private ReactiveAsyncCommand collectModifications = new ReactiveAsyncCommand();

        public class SyncUnitVM : ReactiveObject
        {
            public SyncUnit Model { get; private set; }

            public EventSeriesVM Series { get; private set; }

            public EventVM Event { get; private set; }

            private int modC;

            public int ModificationCount
            {
                get { return modC; }
                private set 
                {
                    this.RaiseAndSetIfChanged(x => x.ModificationCount, ref modC, value);
                }
            }

            public void Append(SyncUnit inc)
            {
                Model.increment(inc);
                ModificationCount += inc.Size;
            }


            public SyncUnitVM(SyncUnit model, Event ev, EventSeries series)
            {
                Model = model;
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
                    var res = new ReplaySubject<SyncUnit>();
                    new Task(() => collectModificationsImpl(res, search.Token)).Start();
                    return res;
                })
                .Subscribe(inc => incrementModificationCount(inc));
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
                .Select(arg => arg as SyncUnit)
                .Where(arg => arg != null)
                .SubscribeOnDispatcher()
                .Subscribe(unit =>
                    {
                        var vm = SyncUnits.Where(v => v.Model == unit).FirstOrDefault();
                        if (vm != null)
                            SyncUnits.Remove(vm);
                    });


            UploadUnit = new ReactiveCommand(canUpload);
            UploadUnit
                .Select(unit => unit as SyncUnitVM)
                .Where(unit => unit != null)
                .Subscribe(unit => backg.startTask<UploadSeriesTask>(unit.Model));
                
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

        private void collectModificationsImpl(ISubject<SyncUnit> outputSubject, CancellationToken cancellation)
        {
            

            using (var ctx = new DiversityDataContext())
            {

                //Modified Series without event not shown

                //Modified Events
                var events = from e in ctx.Events
                             where e.ModificationState == ModificationState.Modified                             
                             select new SyncUnit(e.SeriesID, e.EventID);
                foreach (var ev in events)
                {
                    if (cancellation.IsCancellationRequested)
                        return;
                    outputSubject.OnNext(ev);
                }

                // TODO Modified Properties                

                //Specimen without Units disregarded

                //Modified Units
                var ius = from iu in ctx.IdentificationUnits
                          where iu.ModificationState == ModificationState.Modified                          
                          from spec in ctx.Specimen
                          where spec.CollectionSpecimenID == iu.SpecimenID
                          group iu.UnitID by spec.CollectionEventID into g
                          from ev in ctx.Events
                          where ev.EventID == g.Key
                          select new { Series = ev.SeriesID, Event = ev.EventID, Units = g };
                foreach (var iu in ius)
                {
                    if (cancellation.IsCancellationRequested)                        
                        return;

                    var units = new SyncUnit(iu.Series, iu.Event);
                    units.UnitIDs.AddRange(iu.Units);
                    outputSubject.OnNext(units);
                }

                //Modified Analyses
                var iuas =  from iua in ctx.IdentificationUnitAnalyses
                            where iua.ModificationState == ModificationState.Modified
                            from iu in ctx.IdentificationUnits
                            where iu.UnitID == iua.IdentificationUnitID
                            from spec in ctx.Specimen
                            where spec.CollectionSpecimenID == iu.SpecimenID
                            group iua.IdentificationUnitAnalysisID by spec.CollectionEventID into g
                            from ev in ctx.Events
                            where ev.EventID == g.Key
                            select new { Series = ev.SeriesID, Event = ev.EventID, IUAs = g };
                foreach (var iua in iuas)
                {
                    if (cancellation.IsCancellationRequested)
                        return;

                    var analyses = new SyncUnit(iua.Series, iua.Event);
                    analyses.AnalysisIDs.AddRange(iua.IUAs);
                    outputSubject.OnNext(analyses);
                }
            }
        }

        private void incrementModificationCount(SyncUnit inc)
        {            

            //lock (this)
            //{
                var existing = (from series in SyncUnits
                               where series.Model.SeriesID == inc.SeriesID && series.Model.EventID == inc.EventID
                               select series).FirstOrDefault();
                if (existing != null)
                {
                    existing.Append(inc);
                }
                else
                {
                    var series = Storage.getEventSeriesByID(inc.SeriesID);
                    var ev = Storage.getEventByID(inc.EventID);
                    var vm = new SyncUnitVM(inc,ev,series);
                    SyncUnits.Add(vm);
                }
            //}
        }
    }
}
