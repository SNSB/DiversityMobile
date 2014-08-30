using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace DiversityPhone.Services
{
    public class CascadingDeleter
    {
        private readonly IStoreMultimedia MultimediaStore;

        public CascadingDeleter(IStoreMultimedia Multimedia)
        {
            MultimediaStore = Multimedia;
        }

        private T attachedRowFrom<T>(DiversityDataContext ctx, IQueryOperations<T> operations, T detachedRow) where T : class
        {
            return operations.WhereKeyEquals(ctx.GetTable<T>(), detachedRow)
                .FirstOrDefault();
        }

        public IObservable<Unit> deleteCascadingAsync<T>(T detachedRow) where T : class
        {
            return Observable.Start(() =>
                {
                    using (var ctx = new DiversityDataContext())
                    {
                        if (typeof(T) == typeof(EventSeries))
                        {
                            var attachedRow = attachedRowFrom(ctx, EventSeries.Operations, detachedRow as EventSeries);
                            if (attachedRow != null)
                                deleteSeries(ctx, attachedRow);
                        }
                        else if (typeof(T) == typeof(GeoPointForSeries))
                        {
                            var attachedRow = attachedRowFrom(ctx, GeoPointForSeries.Operations, detachedRow as GeoPointForSeries);
                            if (attachedRow != null)
                                deleteGeoPoint(ctx, attachedRow);
                        }
                        else if (typeof(T) == typeof(Event))
                        {
                            var attachedRow = attachedRowFrom(ctx, Event.Operations, detachedRow as Event);
                            if (attachedRow != null)
                                deleteEvent(ctx, attachedRow);
                        }
                        else if (typeof(T) == typeof(EventProperty))
                        {
                            var attachedRow = attachedRowFrom(ctx, EventProperty.Operations, detachedRow as EventProperty);
                            if (attachedRow != null)
                                deleteProperty(ctx, attachedRow);
                        }
                        else if (typeof(T) == typeof(Specimen))
                        {
                            var attachedRow = attachedRowFrom(ctx, Specimen.Operations, detachedRow as Specimen);
                            if (attachedRow != null)
                                deleteSpecimen(ctx, attachedRow);
                        }
                        else if (typeof(T) == typeof(IdentificationUnit))
                        {
                            var attachedRow = attachedRowFrom(ctx, IdentificationUnit.Operations, detachedRow as IdentificationUnit);
                            if (attachedRow != null)
                                deleteUnit(ctx, attachedRow, true);
                        }
                        else if (typeof(T) == typeof(IdentificationUnitAnalysis))
                        {
                            var attachedRow = attachedRowFrom(ctx, IdentificationUnitAnalysis.Operations, detachedRow as IdentificationUnitAnalysis);
                            if (attachedRow != null)
                                deleteAnalysis(ctx, attachedRow);
                        }
                        else if (typeof(T) == typeof(MultimediaObject))
                        {
                            var attachedRow = attachedRowFrom(ctx, MultimediaObject.Operations, detachedRow as MultimediaObject);
                            if (attachedRow != null)
                                deleteMMO(ctx, attachedRow);
                        }
                        else
                            throw new ArgumentException("Unsupported Type T");

                        ctx.SubmitChanges();
                    }
                });
        }

        private void deleteSeries(DiversityDataContext ctx, EventSeries es)
        {
            foreach (var ev in Queries.Events(es, ctx))
                deleteEvent(ctx, ev);

            foreach (var gp in Queries.GeoPoints(es, ctx))
                deleteGeoPoint(ctx, gp);

            ctx.EventSeries.DeleteOnSubmit(es);
        }

        private void deleteGeoPoint(DiversityDataContext ctx, GeoPointForSeries p)
        {
            ctx.GeoTour.DeleteOnSubmit(p);
        }

        private void deleteEvent(DiversityDataContext ctx, Event ev)
        {
            foreach (var s in Queries.Specimen(ev, ctx))
                deleteSpecimen(ctx, s);

            foreach (var p in Queries.Properties(ev, ctx))
                deleteProperty(ctx, p);

            foreach (var mmo in Queries.Multimedia(ev, ctx))
                deleteMMO(ctx, mmo);

            ctx.Events.DeleteOnSubmit(ev);
        }

        private void deleteSpecimen(DiversityDataContext ctx, Specimen spec)
        {
            foreach (var iu in Queries.Units(spec, ctx))
                deleteUnit(ctx, iu, false);

            foreach (var mmo in Queries.Multimedia(spec, ctx))
                deleteMMO(ctx, mmo);

            ctx.Specimen.DeleteOnSubmit(spec);
        }

        private void deleteUnit(DiversityDataContext ctx, IdentificationUnit iu, bool cascade = false)
        {
            foreach (var an in Queries.Analyses(iu, ctx))
                deleteAnalysis(ctx, an);

            foreach (var mmo in Queries.Multimedia(iu, ctx))
                deleteMMO(ctx, mmo);

            if (cascade)
                foreach (var siu in Queries.SubUnits(iu, ctx))
                    deleteUnit(ctx, siu, cascade);

            ctx.IdentificationUnits.DeleteOnSubmit(iu);
        }

        private void deleteAnalysis(DiversityDataContext ctx, IdentificationUnitAnalysis an)
        {
            ctx.IdentificationUnitAnalyses.DeleteOnSubmit(an);
        }

        private void deleteProperty(DiversityDataContext ctx, EventProperty p)
        {
            ctx.EventProperties.DeleteOnSubmit(p);
        }

        private void deleteMMO(DiversityDataContext ctx, MultimediaObject mmo)
        {
            MultimediaStore.DeleteMultimedia(mmo.Uri);
            ctx.MultimediaObjects.DeleteOnSubmit(mmo);
        }
    }
}