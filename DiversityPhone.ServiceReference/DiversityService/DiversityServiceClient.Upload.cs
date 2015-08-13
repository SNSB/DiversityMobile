using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            var svcProps = new ObservableCollection<Svc.EventProperty>(properties.Select(l => l.ToServiceObject()));
            return DiversityServiceCallObservable((IObservable<Svc.InsertEventCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(ev, Mapping), svc =>
                    WithCredentials(c => svc.InsertEventAsync(ev.ToServiceObject(Mapping), svcProps, c))
                );
        }

        public IObservable<Unit> InsertSpecimen(Specimen spec)
        {
            return DiversityServiceCallObservable((IObservable<Svc.InsertSpecimenCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(spec, Mapping), svc =>
                    WithCredentials(c => svc.InsertSpecimenAsync(spec.ToServiceObject(Mapping), c))
                );
        }

        public IObservable<Unit> InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses)
        {
            var svcLocs = new ObservableCollection<Svc.IdentificationUnitAnalysis>(analyses.Select(l => l.ToServiceObject()));
            return DiversityServiceCallObservable((IObservable<Svc.InsertIdentificationUnitCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(iu, Mapping), svc =>
                    WithCredentials(c => svc.InsertIdentificationUnitAsync(iu.ToServiceObject(Mapping), svcLocs, c))
                );
        }

        public IObservable<Unit> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations)
        {
            var svcLocs = new ObservableCollection<Svc.Localization>(localizations.Select(l => l.ToServiceObject()));
            return DiversityServiceCallObservable((IObservable<Svc.InsertEventSeriesCompletedEventArgs> o) => o
                .Select(x => x.Result)
                .StoreMapping(series, Mapping), svc =>
                    WithCredentials(c => svc.InsertEventSeriesAsync(series.ToServiceObject(), svcLocs, c))
                );
        }

        public IObservable<Unit> InsertMultimediaObject(MultimediaObject mmo)
        { 
            return DiversityServiceCallObservable((IObservable<AsyncCompletedEventArgs> o) => o.Select(_ => Unit.Default), 
                svc =>
                    WithCredentials(c => svc.InsertMMOAsync(mmo.ToServiceObject(Mapping), c))
                );
        }
    }
}