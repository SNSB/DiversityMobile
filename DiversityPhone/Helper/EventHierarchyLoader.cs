using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace DiversityPhone.ViewModels
{
    public class EventHierarchyLoader
    {
        readonly IDiversityServiceClient Service;
        readonly IFieldDataService Storage;
        readonly IKeyMappingService Mappings;
        readonly IScheduler ThreadPool;

        public EventHierarchyLoader(
            IDiversityServiceClient Service,
            IFieldDataService Storage,
            IKeyMappingService Mappings,
            [ThreadPool] IScheduler ThreadPool
            )
        {
            this.Service = Service;
            this.Storage = Storage;
            this.Mappings = Mappings;
            this.ThreadPool = ThreadPool;
        }

        public IObservable<Unit> downloadAndStoreDependencies(Event ev)
        {
            if (ev == null)
                throw new ArgumentNullException();

            //Avoid undesirable interactions
            ev = ev.MemberwiseClone();


            IObservable<EventSeries> series_future = getOrDownloadSeries(ev.SeriesID);
            IObservable<Event> event_future = addEvent(series_future, ev);
            IObservable<Unit> props_future = downloadProperties(event_future);
            IObservable<Specimen> specimen_future = downloadSpecimen(event_future);
            IObservable<IdentificationUnit> iu_future = downloadUnits(specimen_future);

            IObservable<Unit> an_future = downloadAnalyses(iu_future);


            return
            Observable.Merge(
                series_future.Select(_ => Unit.Default),
                event_future.Select(_ => Unit.Default),
                props_future,
                specimen_future.Select(_ => Unit.Default),
                iu_future.Select(_ => Unit.Default),
                an_future
                );
        }


        public IObservable<EventSeries> getOrDownloadSeries(int? collectionSeriesID)
        {
            var localKey = (collectionSeriesID.HasValue) ? Mappings.ResolveToLocalKey(DBObjectType.EventSeries, collectionSeriesID.Value) : null;
            if (localKey.HasValue)
            {
                return Observable.Return(Storage.get<EventSeries>(localKey));
            }
            else if (!collectionSeriesID.HasValue)
            {
                return Observable.Return(EventSeries.NoEventSeries);
            }
            else
            {
                var series_future = Service.GetEventSeriesByID(collectionSeriesID.Value);
                var locs_future = Service.GetEventSeriesLocalizations(collectionSeriesID.Value);

                var insertion_future =
                    series_future
                        .Do(Storage.add)
                        .Zip(locs_future,
                            (es, locs) =>
                            {
                                foreach (var loc in locs)
                                {
                                    loc.RelatedID = es.SeriesID.Value;
                                    Storage.add(loc);
                                }
                                return es;
                            }).Replay()
                            .RefCount();

                return insertion_future;
            }
        }

        public IObservable<Event> addEvent(IObservable<EventSeries> series_future, Event ev)
        {
            var insertion_future =
                series_future
                .Select(es =>
                {
                    ev.SeriesID = es.SeriesID;
                    Storage.add(ev);
                    return ev;
                }).Replay()
                .RefCount();

            return insertion_future;
        }

        public IObservable<Unit> downloadProperties(IObservable<Event> event_future)
        {
            var insertion_future =
            event_future
                .SelectMany(ev => Service.GetEventProperties(ev.CollectionEventID.Value)
                    .Do(props =>
                    {
                        foreach (var prop in props)
                        {
                            prop.EventID = ev.EventID;
                            Storage.add(prop);
                        }
                    })
                )
                .Select(_ => Unit.Default)
                .Replay()
                .RefCount();

            return insertion_future;
        }

        public IObservable<Specimen> downloadSpecimen(IObservable<Event> event_future)
        {
            var insertion_future =
                event_future
                .SelectMany(ev => Service.GetSpecimenForEvent(ev.CollectionEventID.Value)
                    .SelectMany(specimen => specimen.ToObservable(ThreadPool))
                )
                .CombineLatest(event_future,
                    (spec, ev) =>
                    {
                        spec.EventID = ev.EventID;
                        Storage.add(spec);
                        return spec;
                    }).Replay()
                    .RefCount();

            return insertion_future;
        }

        public IEnumerable<IdentificationUnit> insertUnitsHierarchical(ILookup<int?, IdentificationUnit> tree)
        {
            Queue<IdentificationUnit> todo = new Queue<IdentificationUnit>(tree[null]);

            while (todo.Count > 0)
            {
                var iu = todo.Dequeue();

                Storage.add(iu);

                foreach (var subu in tree[iu.CollectionUnitID])
                {
                    subu.RelatedUnitID = iu.UnitID;
                    todo.Enqueue(subu);
                }

                yield return iu;
            }
        }

        public IObservable<IdentificationUnit> downloadUnits(IObservable<Specimen> specimen_future)
        {
            var insertion_future =
                specimen_future
                .SelectMany(spec => Service.GetIdentificationUnitsForSpecimen(spec.CollectionSpecimenID.Value)
                    .Select(ius =>
                    {
                        var unitList = ius.ToList();
                        foreach (var iu in unitList)
                        {
                            iu.SpecimenID = spec.SpecimenID;
                        }

                        return unitList;
                    })
                    .Select(ius => ius.ToLookup(iu => iu.RelatedUnitID))
                    .SelectMany(tree => insertUnitsHierarchical(tree).ToObservable(ThreadPool))
                ).Replay()
                .RefCount();

            return insertion_future;
        }

        public IObservable<Unit> downloadAnalyses(IObservable<IdentificationUnit> unit_future)
        {
            var insertion_future =
                unit_future
                    .SelectMany(iu => Service.GetAnalysesForIU(iu.CollectionUnitID.Value).SelectMany(ans => ans)
                        .Do(an =>
                        {
                            an.UnitID = iu.UnitID;
                            Storage.add(an);
                        })
                    )
                    .Select(_ => Unit.Default)
                    .Replay()
                    .RefCount();

            return insertion_future;
        }
    }
}
