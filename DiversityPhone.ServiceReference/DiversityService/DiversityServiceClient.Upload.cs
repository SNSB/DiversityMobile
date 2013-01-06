
using System.Collections.Generic;
using DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using Svc = DiversityPhone.DiversityService;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Reactive;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        public IObservable<int> InsertEvent(Event ev, IEnumerable<EventProperty> properties)
        {
            var res = ToResultObservable(
                InsertEVCompleted
                .Where(p => p.EventArgs.UserState == ev)
                .Select(p => p.EventArgs.Result)
                .Do(id => Mapping.AddMapping(DBObjectType.Event, ev.EventID, id))
                );

            var svcProps = new ObservableCollection<Svc.EventProperty>(properties.Select(l => l.ToServiceObject()));
            _svc.InsertEventAsync(ev.ToServiceObject(Mapping), svcProps, this.GetCreds(), ev);
            return res;
        }

        public IObservable<int> InsertSpecimen(Specimen spec)
        {
            var res = ToResultObservable(
                InsertSPCompleted
                .Where(p => p.EventArgs.UserState == spec)
                .Select(p => p.EventArgs.Result)
                .Do(id => Mapping.AddMapping(DBObjectType.Specimen, spec.SpecimenID, id))
                );
     
            _svc.InsertSpecimenAsync(spec.ToServiceObject(Mapping), this.GetCreds(), spec);
            return res;
        }

        public IObservable<int> InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses)
        {
            var res = ToResultObservable(
                InsertIUCompleted
                .Where(p => p.EventArgs.UserState == iu)
                .Select(p => p.EventArgs.Result)
                .Do(id => Mapping.AddMapping(DBObjectType.IdentificationUnit, iu.UnitID, id))
                );

            var svcLocs = new ObservableCollection<Svc.IdentificationUnitAnalysis>(analyses.Select(l => l.ToServiceObject()));
            _svc.InsertIdentificationUnitAsync(iu.ToServiceObject(Mapping), svcLocs, this.GetCreds(), iu);
            return res;
        }

        public IObservable<int> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations)
        {
            if (!series.SeriesID.HasValue)
                throw new ArgumentException("series");

            var res = ToResultObservable(
                InsertESCompleted
                .Where(p => p.EventArgs.UserState == series)
                .Select(p => p.EventArgs.Result)
                .Do(id => Mapping.AddMapping(DBObjectType.EventSeries, series.SeriesID.Value, id))
                );
            
            var svcLocs = new ObservableCollection<Svc.Localization>(localizations.Select(l => l.ToServiceObject()));
            _svc.InsertEventSeriesAsync(series.ToServiceObject(),svcLocs, this.GetCreds(), series);
            return res;
        }


        public IObservable<Unit> InsertMultimediaObject(MultimediaObject mmo)
        {
            var res = ToResultObservable(
                InsertMMOCompleted
                .Where(p => p.EventArgs.UserState == mmo)
                .Select(_ => Unit.Default)
                );
            
            var repoMmo = mmo.ToServiceObject(Mapping);
            _svc.InsertMMOAsync(repoMmo, this.GetCreds(), mmo);
            return res;
        }
    }
}
