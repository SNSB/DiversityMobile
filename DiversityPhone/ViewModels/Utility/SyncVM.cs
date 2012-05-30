using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Funq;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Collections.Generic;
using DiversityPhone.Services;
using System.Linq;
using DiversityPhone.Model;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels.Utility
{
    public class SyncVM : PageViewModel
    {
        private IFieldDataService Storage;


        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        private bool _stopSearching = false;
        
        public ReactiveCollection<SyncUnitVM> SyncUnits { get; private set; }        

        private ReactiveAsyncCommand collectModifications = new ReactiveAsyncCommand();

        public class SyncUnitVM : EventSeriesVM
        {
            public override string Description
            {
                get
                {
                    return string.Format("{0} ({1})",base.Description,ModificationCount);
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


            public SyncUnitVM(EventSeries model) : base(model)
            {
                   
            }
        }

        public SyncVM(Container ioc)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            SyncUnits = new ReactiveCollection<SyncUnitVM>();
            StateObservable
                .Subscribe(_ => _stopSearching = false);
        }

        public override void SaveState()
        {
            _stopSearching = true;
        }

        private void collectModificationsImpl()
        {
            SyncUnits.Clear();

            using (var ctx = new DiversityDataContext())
            {

                //Modified Series
                var series = from s in ctx.EventSeries
                             where s.ModificationState == ModificationState.Modified
                             select s.SeriesID;
                foreach (var s in series)
                {
                    if (_stopSearching)
                        return;
                    incrementModificationCount(s, 1);
                }

                var events = from e in ctx.Events
                             where e.ModificationState == ModificationState.Modified
                             group e by e.SeriesID into g
                             select new { ID = g.Key, Count = g.Count() };
                foreach (var ev in events)
                {
                    if (_stopSearching)
                        return;
                    incrementModificationCount(ev.ID, ev.Count);
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
                    if (_stopSearching)
                        return;
                    incrementModificationCount(iu.ID, iu.Count);
                }
            }
        }

        private void incrementModificationCount(int? seriesID, int increment)
        {
            lock (this)
            {
                var existing = (from series in SyncUnits
                               where series.Model.SeriesID == seriesID
                               select series).FirstOrDefault();
                if (existing != null)
                {
                    existing.ModificationCount += increment;
                }
                else
                {
                    
                    var series = new SyncUnitVM(Storage.getEventSeriesByID(seriesID))
                    {                        
                        ModificationCount = increment
                    };                    
                }
            }
        }
    }
}
