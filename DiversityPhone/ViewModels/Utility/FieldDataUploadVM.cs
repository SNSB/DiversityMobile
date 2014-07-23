namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using DiversityPhone.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;

    public class FieldDataUploadVM : IUploadVM<IElementVM>
    {
        readonly IScheduler ThreadPool;
        readonly IDiversityServiceClient Service;
        readonly IFieldDataService Storage;

        public MultipleSelectionHelper<IElementVM> Items { get; private set; }

        public IObservable<Unit> Refresh()
        {
            return Observable.Create<Unit>(obs =>
                {
                    var cancelSource = new CancellationTokenSource();
                    var cancel = cancelSource.Token;
                    Items.OnNext(
                        collectModificationsImpl()
                            .ToObservable(ThreadPool)
                            .TakeWhile(_ => !cancel.IsCancellationRequested)
                            .Finally(obs.OnCompleted)
                            );
                    return Disposable.Create(cancelSource.Cancel);
                });
        }

        public IObservable<Tuple<int, int>> Upload()
        {
            return Observable.Defer(() => Items.SelectedItems)
                       .SelectMany(elements => ObservableMixin.StartWithCancellation<Tuple<int, int>>((cancel, obs) =>
                       {
                           var totalCount = elements.Count();
                           int idx = 0;
                           obs.OnNext(Tuple.Create(idx, totalCount));
                           foreach (var e in elements)
                           {
                               uploadTree(e)
                                   .TakeWhile(_ => cancel.IsCancellationRequested)
                                   .Do(_ =>
                                   {
                                       ++totalCount;
                                       ++idx;
                                       obs.OnNext(Tuple.Create(++idx, totalCount));
                                   }).LastOrDefault();
                               Items.Remove(e);
                               if (cancel.IsCancellationRequested) return;

                               obs.OnNext(Tuple.Create(++idx, totalCount));
                           }
                       })).Publish().RefCount();
        }

        public FieldDataUploadVM(
            IDiversityServiceClient Service,
            IFieldDataService Storage,
            [ThreadPool] IScheduler ThreadPool,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            this.ThreadPool = ThreadPool;
            this.Service = Service;
            this.Storage = Storage;

            Items = new MultipleSelectionHelper<IElementVM>();
        }


        private IObservable<Unit> uploadES(EventSeries es)
        {
            return Service.InsertEventSeries(es, Storage.getGeoPointsForSeries(es.SeriesID).Select(gp => gp as ILocalizable))
                    .SelectMany(_ => Storage.getEventsForSeries(es).Select(ev => uploadEV(ev)))
                    .SelectMany(obs => obs);
        }

        private IObservable<Unit> uploadEV(Event ev)
        {
            return Service.InsertEvent(ev, Storage.getPropertiesForEvent(ev.EventID))
                    .SelectMany(_ => Storage.getSpecimenForEvent(ev).Select(s => uploadSpecimen(s)))
                    .SelectMany(x => x);
        }

        private IObservable<Unit> uploadSpecimen(Specimen s)
        {
            return Observable.Start(() => Storage.getTopLevelIUForSpecimen(s.SpecimenID))
                // Only Upload Specimen with at least one IU
                .Where(ius => ius != null && ius.Any())
                .SelectMany(ius => 
                    Service.InsertSpecimen(s)
                           .SelectMany(_ => ius)                           
                )
                .SelectMany(iu => uploadIU(iu));
        }

        private IObservable<Unit> uploadIU(IdentificationUnit iu)
        {
            return Service.InsertIdentificationUnit(iu, Storage.getIUANForIU(iu))
                .SelectMany(_ => Storage.getSubUnits(iu).Select(sub => uploadIU(sub)))
                .SelectMany(x => x);
        }
        private IObservable<Unit> uploadTree(IElementVM vm)
        {
            var model = vm.Model;
            IObservable<Unit> res = Observable.Empty<Unit>();

            if (model is EventSeries)
                res = uploadES(model as EventSeries);
            if (model is Event)
                res = uploadEV(model as Event);
            if (model is Specimen)
                res = uploadSpecimen(model as Specimen);
            if (model is IdentificationUnit)
                res = uploadIU(model as IdentificationUnit);

            return res;
        }

        private IEnumerable<IElementVM> collectModificationsImpl()
        {
            using (var ctx = new DiversityDataContext())
            {
                // Finished Series
                foreach (var i in (from es in ctx.EventSeries
                                   where es.CollectionSeriesID == null && es.SeriesEnd != null
                                   select new EventSeriesVM(es) as IElementVM))
                    yield return i;
                // Events in Uploaded Series
                foreach (var i in (from es in ctx.EventSeries
                                   where es.CollectionSeriesID != null
                                   join ev in ctx.Events on es.SeriesID equals ev.SeriesID
                                   where ev.CollectionEventID == null
                                   select new EventVM(ev) as IElementVM))
                    yield return i;
                // Events in NoEventSeries
                foreach (var i in (from ev in ctx.Events
                                   where ev.SeriesID == null && ev.CollectionEventID == null
                                   select new EventVM(ev) as IElementVM))
                    yield return i;
                // Specimen in Uploaded Series
                foreach (var i in (from ev in ctx.Events
                                   where ev.CollectionEventID != null
                                   join s in ctx.Specimen on ev.EventID equals s.EventID
                                   let hasUnits = (from iu in ctx.IdentificationUnits
                                                   where iu.SpecimenID == s.SpecimenID
                                                   select Unit.Default).Any()                                                      
                                   where s.CollectionSpecimenID == null && hasUnits
                                   select new SpecimenVM(s) as IElementVM))
                {
                    yield return i;
                }

                foreach (var i in (from iu in
                                       //New IU with parent Spec Uploaded
                                       (from s in ctx.Specimen
                                        where s.CollectionSpecimenID != null
                                        join iu in ctx.IdentificationUnits on s.SpecimenID equals iu.SpecimenID
                                        where iu.CollectionUnitID == null
                                        && iu.RelatedUnitID == null
                                        select iu)
                                           //New IU with parent Unit uploaded
                                   .Union(from u in ctx.IdentificationUnits
                                          where u.CollectionUnitID != null
                                          join sub in ctx.IdentificationUnits on u.UnitID equals sub.RelatedUnitID
                                          where sub.CollectionUnitID == null
                                          select sub)
                                   select new IdentificationUnitVM(iu) as IElementVM))
                    yield return i;
            }

        }



    }
}
