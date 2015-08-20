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
        private readonly IScheduler ThreadPool;
        private readonly IDiversityServiceClient Service;
        private readonly IFieldDataService Storage;

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

        public IObservable<IItemsProgress> Upload()
        {
            return Observable.Defer(() => Items.SelectedItems)
                       .SelectMany(elements => ObservableMixin.StartWithCancellation<IItemsProgress>((cancel, obs) =>
                       {
                           var progress = new ItemProgress();
                           progress.ItemsTotal = elements.Count();
                           progress.ItemsDone = 0;
                           
                           foreach (var e in elements)
                           {
                               uploadTree(e, progress)
                                   .TakeWhile(_ => !cancel.IsCancellationRequested)
                                   .Do(progress.IncrementDone)
                                   .Select(_ => progress as IItemsProgress)
                                   .Subscribe(obs);
                               Items.Remove(e);
                               if (cancel.IsCancellationRequested) return;
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

        private IObservable<Unit> uploadES(EventSeries es, ItemProgress progress)
        {
            return Service.InsertEventSeries(es, Storage.getGeoPointsForSeries(es.SeriesID).Select(gp => gp as ILocalizable))
                    .RefCount()
                    .Select(_ => Storage.getEventsForSeries(es))
                    .Do(progress.IncrementTotal)
                    .SelectMany(events => events.Select(ev => uploadEV(ev, progress)))
                    .SelectMany(obs => obs);
        }

        private IObservable<Unit> uploadEV(Event ev, ItemProgress progress)
        {
            return Service.InsertEvent(ev, Storage.getPropertiesForEvent(ev.EventID))
                    .RefCount()
                    .Select(_ => Storage.getSpecimenForEvent(ev))
                    .Do(progress.IncrementTotal)
                    .SelectMany(series => series.Select(s => uploadSpecimen(s, progress)))
                    .SelectMany(x => x);
        }

        private IObservable<Unit> uploadSpecimen(Specimen s, ItemProgress progress)
        {
            return Observable.Start(() =>
                {
                    var ius = Storage.getTopLevelIUForSpecimen(s.SpecimenID);
                    var mmos = Storage.getMultimediaForObject(s);

                    // Only Insert Specimen with at least one IU or MMO
                    if (ius.Any() || mmos.Any())
                    {
                        return ius;
                    }
                    return null;
                })
                .Where(x => x != null)
                .Do(progress.IncrementTotal)
                .SelectMany(ius =>
                    Service.InsertSpecimen(s)
                           .RefCount()
                           .SelectMany(_ => ius)
                )
                .SelectMany(iu => uploadIU(iu, progress));
        }

        private IObservable<Unit> uploadIU(IdentificationUnit iu, ItemProgress progress)
        {
            return Service.InsertIdentificationUnit(iu, Storage.getIUANForIU(iu))
                .RefCount()
                .SelectMany(_ => Storage.getSubUnits(iu).Select(sub => uploadIU(sub, progress)))
                .SelectMany(x => x);
        }

        private IObservable<Unit> uploadTree(IElementVM vm, ItemProgress progress)
        {
            var model = vm.Model;
            IObservable<Unit> res = Observable.Empty<Unit>();

            if (model is EventSeries)
                res = uploadES(model as EventSeries, progress);
            if (model is Event)
                res = uploadEV(model as Event, progress);
            if (model is Specimen)
                res = uploadSpecimen(model as Specimen, progress);
            if (model is IdentificationUnit)
                res = uploadIU(model as IdentificationUnit, progress);

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
                                   let hasMMO = (from mmo in ctx.MultimediaObjects
                                                 where mmo.OwnerType == DBObjectType.Specimen &&
                                                       mmo.RelatedId == s.SpecimenID
                                                 select Unit.Default).Any()
                                   where s.CollectionSpecimenID == null && (hasUnits || hasMMO)
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