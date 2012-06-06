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

namespace DiversityPhone.ViewModels.Utility
{
    public class SyncVM : PageViewModel
    {
        private IFieldDataService Storage;


        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        private CancellationTokenSource search = null;
        
        public ReactiveCollection<SyncUnitVM> SyncUnits { get; private set; }        

        private ReactiveAsyncCommand collectModifications = new ReactiveAsyncCommand();

        public class SyncUnitVM : EventSeriesVM
        {
            public override string Description
            {
                get
                {
                    return string.Format("{0} ({1})", base.Description, ModificationCount);
                }
            }

            private int modC;

            public int ModificationCount
            {
                get { return modC; }
                set 
                { 
                    modC = value;
                    this.RaisePropertyChanged(x => x.Description);
                }
            }


            public SyncUnitVM(EventSeries model) : base(MessageBus.Current, model, Page.Current)
            {
                
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
                    var res = new ReplaySubject<SyncUnitIncrement>();
                    new Task(() => collectModificationsImpl(res, search.Token)).Start();
                    return res;
                })
                .Subscribe(inc => incrementModificationCount(inc));
            _IsBusy = this.ObservableToProperty(collectModifications.ItemsInflight.Select(i => i > 0),x => x.IsBusy, false);
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

        private void collectModificationsImpl(ISubject<SyncUnitIncrement> outputSubject, CancellationToken cancellation)
        {
            

            using (var ctx = new DiversityDataContext())
            {

                //Modified Series
                var series = from s in ctx.EventSeries
                             where s.ModificationState == ModificationState.Modified
                             select s.SeriesID;
                foreach (var s in series)
                {
                    if (cancellation.IsCancellationRequested)
                        return;
                    outputSubject.OnNext(new SyncUnitIncrement() { Unit = s, Increment = 1 });
                }

                var events = from e in ctx.Events
                             where e.ModificationState == ModificationState.Modified
                             group e by e.SeriesID into g
                             select new { ID = g.Key, Count = g.Count() };
                foreach (var ev in events)
                {
                    if (cancellation.IsCancellationRequested)
                        return;
                    outputSubject.OnNext(new SyncUnitIncrement() { Unit = ev.ID, Increment = ev.Count });
                }

                var ius = from iu in ctx.IdentificationUnits
                          where iu.ModificationState == ModificationState.Modified
                          group iu by iu.SpecimenID into g
                          from spec in ctx.Specimen
                          where spec.CollectionSpecimenID == g.Key
                          from ev in ctx.Events
                          where ev.EventID == spec.CollectionEventID
                          select new { ID = ev.SeriesID, Count = g.Count() };
                foreach (var iu in ius)
                {
                    if (cancellation.IsCancellationRequested)                        
                        return;
                    outputSubject.OnNext(new SyncUnitIncrement() { Unit = iu.ID, Increment = iu.Count });
                }
            }
        }

        private void incrementModificationCount(SyncUnitIncrement inc)
        {
            var seriesID = inc.Unit;
            var increment = inc.Increment;

            //lock (this)
            //{
                var existing = (from series in SyncUnits
                               where series.Model.SeriesID == seriesID
                               select series).FirstOrDefault();
                if (existing != null)
                {
                    existing.ModificationCount += increment;
                }
                else
                {
                    var series = (seriesID.HasValue) ? Storage.getEventSeriesByID(seriesID.Value) : EventSeries.NoEventSeries;
                    var vm = new SyncUnitVM(series)
                    {                        
                        ModificationCount = increment
                    };
                    SyncUnits.Add(vm);
                }
            //}
        }
    }
}
