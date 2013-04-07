
using System.Collections.Generic;
using DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using Svc = DiversityPhone.DiversityService;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Reactive;
using DiversityPhone.Interface;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        public IObservable<Unit> InsertEvent(Event ev, IEnumerable<EventProperty> properties)
        {
            var res = InsertEVCompleted.FilterByUserState(ev)
                .PipeErrors()
                .Select(p => p.Result)
                .StoreMapping(ev, Mapping)
                .ReplayOnlyFirst();
                

            var svcProps = new ObservableCollection<Svc.EventProperty>(properties.Select(l => l.ToServiceObject()));
            _svc.InsertEventAsync(ev.ToServiceObject(Mapping), svcProps, this.GetCreds(), ev);
            return res;
        }

        public IObservable<Unit> InsertSpecimen(Specimen spec)
        {
            var res = InsertSPCompleted.FilterByUserState(spec)
                .PipeErrors()
                .Select(p => p.Result)
                .StoreMapping(spec, Mapping)
                .ReplayOnlyFirst();

            _svc.InsertSpecimenAsync(spec.ToServiceObject(Mapping), this.GetCreds(), spec);
            return res;
        }

        public IObservable<Unit> InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses)
        {
            var res = InsertIUCompleted.FilterByUserState(iu)
                .PipeErrors()
                .Select(p => p.Result)
                .StoreMapping(iu, Mapping)
                .ReplayOnlyFirst();

            var svcLocs = new ObservableCollection<Svc.IdentificationUnitAnalysis>(analyses.Select(l => l.ToServiceObject()));
            _svc.InsertIdentificationUnitAsync(iu.ToServiceObject(Mapping), svcLocs, this.GetCreds(), iu);
            return res;
        }

        public IObservable<Unit> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations)
        {
            if (!series.SeriesID.HasValue)
                throw new ArgumentException("series");

            var res = InsertESCompleted.FilterByUserState(series)
                .PipeErrors()
                .Select(p => p.Result)
                .StoreMapping(series, Mapping)
                .ReplayOnlyFirst();

            var svcLocs = new ObservableCollection<Svc.Localization>(localizations.Select(l => l.ToServiceObject()));
            _svc.InsertEventSeriesAsync(series.ToServiceObject(), svcLocs, this.GetCreds(), series);
            return res;
        }


        public IObservable<Unit> InsertMultimediaObject(MultimediaObject mmo)
        {
            var res = InsertMMOCompleted.MakeObservableServiceResultSingle(mmo)
                .Select(_ => Unit.Default);

            var repoMmo = mmo.ToServiceObject(Mapping);
            _svc.InsertMMOAsync(repoMmo, this.GetCreds(), mmo);
            return res;
        }
    }
}
