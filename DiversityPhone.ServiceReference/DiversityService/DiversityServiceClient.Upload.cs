using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Svc = DiversityPhone.DiversityService;

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
            WithCredentials(c => _svc.InsertEventAsync(ev.ToServiceObject(Mapping), svcProps, c, ev));
            return res;
        }

        public IObservable<Unit> InsertSpecimen(Specimen spec)
        {
            var res = InsertSPCompleted.FilterByUserState(spec)
                .PipeErrors()
                .Select(p => p.Result)
                .StoreMapping(spec, Mapping)
                .ReplayOnlyFirst();

            WithCredentials(c => _svc.InsertSpecimenAsync(spec.ToServiceObject(Mapping), c, spec));
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
            WithCredentials(c => _svc.InsertIdentificationUnitAsync(iu.ToServiceObject(Mapping), svcLocs, c, iu));
            return res;
        }

        public IObservable<Unit> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations)
        {
            var res = InsertESCompleted.FilterByUserState(series)
                .PipeErrors()
                .Select(p => p.Result)
                .StoreMapping(series, Mapping)
                .ReplayOnlyFirst();

            var svcLocs = new ObservableCollection<Svc.Localization>(localizations.Select(l => l.ToServiceObject()));
            WithCredentials(c => _svc.InsertEventSeriesAsync(series.ToServiceObject(), svcLocs, c, series));
            return res;
        }

        public IObservable<Unit> InsertMultimediaObject(MultimediaObject mmo)
        {
            var res = InsertMMOCompleted.MakeObservableServiceResultSingle(mmo)
                .Select(_ => Unit.Default);

            var repoMmo = mmo.ToServiceObject(Mapping);
            WithCredentials(c => _svc.InsertMMOAsync(repoMmo, c, mmo));
            return res;
        }
    }
}