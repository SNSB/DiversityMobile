using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    partial class OfflineStorage
    {
        private static class CascadingDelete
        {
            private static DataLoadOptions cascadeOptions()
            {
                var load = new DataLoadOptions();
                load.LoadWith<EventSeries>(es => es.Events);
                load.LoadWith<Event>(ev => ev.Specimen);
                load.LoadWith<Event>(ev => ev.Properties);
                load.LoadWith<Specimen>(s => s.Units);
                load.LoadWith<IdentificationUnit>(iu => iu.Analyses);

                return load;
            }

            private static IQueryable<MultimediaObject> getMMOs(DiversityDataContext ctx, IMultimediaOwner owner)
            {
                return from mmo in ctx.MultimediaObjects
                       where mmo.OwnerType == owner.OwnerType && mmo.RelatedId == owner.OwnerID
                       select mmo;
            }

            private static T attachedRowFrom<T>(DiversityDataContext ctx, IQueryOperations<T> operations, T detachedRow) where T : class
            {  
                return operations.WhereKeyEquals(ctx.GetTable<T>(), detachedRow)
                    .FirstOrDefault();
            }

            public static IObservable<Unit> deleteCascadingAsync<T>(T detachedRow) where T : class
            {
                return Observable.Start(() =>
                    {
                        using (var ctx = new DiversityDataContext())
                        {
                            if (typeof(T) == typeof(EventSeries))
                            {
                                var attachedRow = attachedRowFrom(ctx, EventSeries.Operations, detachedRow as EventSeries);
                                if(attachedRow != null)
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

                            try
                            {
                                ctx.SubmitChanges();
                            }
                            catch (Exception)
                            {
                                Debugger.Break();                                
                            }
                        }
                    });
                        
            }

            private static void deleteSeries(DiversityDataContext ctx, EventSeries es)
            {
                foreach (var ev in es.Events)
                    deleteEvent(ctx, ev);

                ctx.EventSeries.DeleteOnSubmit(es);
            }

            private static void deleteGeoPoint(DiversityDataContext ctx, GeoPointForSeries p)
            {
                ctx.GeoTour.DeleteOnSubmit(p);
            }

            private static void deleteEvent(DiversityDataContext ctx, Event ev)
            {
                foreach (var s in ev.Specimen)
                    deleteSpecimen(ctx, s);

                foreach (var p in ev.Properties)
                    deleteProperty(ctx, p);

                foreach (var mmo in getMMOs(ctx, ev))
                    deleteMMO(ctx, mmo);

                ctx.Events.DeleteOnSubmit(ev);
            }

            private static void deleteSpecimen(DiversityDataContext ctx, Specimen spec)
            {
                foreach (var iu in spec.Units)
                    deleteUnit(ctx, iu, false);

                foreach (var mmo in getMMOs(ctx, spec))
                    deleteMMO(ctx, mmo);

                ctx.Specimen.DeleteOnSubmit(spec);
            }

            private static void deleteUnit(DiversityDataContext ctx, IdentificationUnit iu, bool cascade = false)
            {
                foreach (var an in iu.Analyses)
                    deleteAnalysis(ctx, an);

                foreach (var mmo in getMMOs(ctx, iu))
                    deleteMMO(ctx, mmo);

                if (cascade)
                    foreach (var siu in iu.SubUnits)
                        deleteUnit(ctx, siu, cascade);

                ctx.IdentificationUnits.DeleteOnSubmit(iu);


            }

            private static void deleteAnalysis(DiversityDataContext ctx, IdentificationUnitAnalysis an)
            {
                ctx.IdentificationUnitAnalyses.DeleteOnSubmit(an);
            }

            private static void deleteProperty(DiversityDataContext ctx, EventProperty p)
            {
                ctx.EventProperties.DeleteOnSubmit(p);
            }

            private static void deleteMMO(DiversityDataContext ctx, MultimediaObject mmo)
            {
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(mmo.Uri))
                {
                    try
                    {
                        myStore.DeleteFile(mmo.Uri);
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }
                ctx.MultimediaObjects.DeleteOnSubmit(mmo);
            }
        }        
    }
}
