using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        public IConnectableObservable<Unit> InsertEvent(Event ev, IEnumerable<EventProperty> properties)
        {
            var svcProps = new ObservableCollection<Svc.EventProperty>(properties.Select(l => l.ToServiceObject()));
            return DiversityConnectableServiceCall((IObservable<Svc.InsertEventCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(ev, Mapping), svc =>
                    WithCredentials(c => svc.InsertEventAsync(ev.ToServiceObject(Mapping), svcProps, c))
                );
        }

        public IConnectableObservable<Unit> InsertSpecimen(Specimen spec)
        {
            return DiversityConnectableServiceCall((IObservable<Svc.InsertSpecimenCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(spec, Mapping), svc =>
                    WithCredentials(c => svc.InsertSpecimenAsync(spec.ToServiceObject(Mapping), c))
                );
        }

        public IConnectableObservable<Unit> InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses)
        {
            var svcLocs = new ObservableCollection<Svc.IdentificationUnitAnalysis>(analyses.Select(l => l.ToServiceObject()));
            return DiversityConnectableServiceCall((IObservable<Svc.InsertIdentificationUnitCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(iu, Mapping), svc =>
                    WithCredentials(c => svc.InsertIdentificationUnitAsync(iu.ToServiceObject(Mapping), svcLocs, c))
                );
        }

        public IConnectableObservable<Unit> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations)
        {
            var svcLocs = new ObservableCollection<Svc.Localization>(localizations.Select(l => l.ToServiceObject()));
            return DiversityConnectableServiceCall((IObservable<Svc.InsertEventSeriesCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(series, Mapping), svc =>
                    WithCredentials(c => svc.InsertEventSeriesAsync(series.ToServiceObject(), svcLocs, c))
                );
        }

        public IConnectableObservable<Unit> InsertMultimediaObject(MultimediaObject mmo)
        { 
            return DiversityConnectableServiceCall((IObservable<AsyncCompletedEventArgs> o) => o.Select(_ => Unit.Default), 
                svc =>
                    WithCredentials(c => svc.InsertMMOAsync(mmo.ToServiceObject(Mapping), c))
                );
        }
    }
}