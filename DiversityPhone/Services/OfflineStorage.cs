namespace DiversityPhone.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;

    public class OfflineStorage : IOfflineStorage
    {
        public OfflineStorage()
        {
            using (var context = new DiversityDataContext())
            {
                if (!context.DatabaseExists())
                    context.CreateDatabase();
            }


        }

        public void addEventSeries(global::DiversityPhone.Model.EventSeries newSeries)
        {
            if (newSeries.IsModified != null || newSeries.SeriesID != default(int))
                throw new InvalidOperationException("Series is not new!");

            using (var ctx = new DiversityDataContext())
            {
                newSeries.SeriesID = findFreeEventSeriesID(ctx);
                ctx.EventSeries.InsertOnSubmit(newSeries);
                ctx.SubmitChanges();
            }
        }

        public IList<Specimen> getAllSpecimen()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Specimen>(ctx.Specimen);
        }

        public IList<Specimen> getSpecimenForEvent(Event ev)
        {
            var ctx = new DiversityDataContext();
            return new LightList<Specimen>(from spec in ctx.Specimen
                                           where spec.CollectionEventID == ev.EventID
                                           select spec);
        }

        public IList<EventSeries> getAllEventSeries()
        {
            var ctx = new DiversityDataContext();
            return new LightList<EventSeries>(ctx.EventSeries);
        }

        private static int findFreeEventSeriesID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.EventSeries.Any())
                min = (from es in ctx.EventSeries select es.SeriesID).Min();
            return (min > -1) ? -1 : min - 1;
        }

        private static int findFreeEventID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.Events.Any())
                min = (from ev in ctx.Events select ev.EventID).Min();
            return (min > -1) ? -1 : min - 1;
        }
        private static int findFreeSpecimenID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.Specimen.Any())
                min = (from spec in ctx.Specimen select spec.CollectionSpecimenID).Min();
            return (min > -1) ? -1 : min - 1;
        }

        private static int findFreeUnitID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.IdentificationUnits.Any())
                min = (from iu in ctx.IdentificationUnits select iu.UnitID).Min();
            return (min > -1) ? -1 : min - 1;
        }




        public IList<EventSeries> getEventSeriesByDescription(string query)
        {
            throw new NotImplementedException();
        }

        public IList<EventSeries> getNewEventSeries()
        {
            throw new NotImplementedException();
        }

        public EventSeries getEventSeriesByID(int id)
        {
            throw new NotImplementedException();
        }


        public IList<Event> getAllEvents()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Event>(ctx.Events);

        }

        public void addEvent(Event ev)
        {
            using (var ctx = new DiversityDataContext())
            {
                if (ev.IsModified == null)
                    ev.EventID = findFreeEventID(ctx);
                ctx.Events.InsertOnSubmit(ev);
                ctx.SubmitChanges();
            }
        }

        public void addTerms(IEnumerable<Term> terms)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.Terms.InsertAllOnSubmit(terms);
                ctx.SubmitChanges();
            }

            sampleData();
        }


        public IList<Term> getTerms(int source)
        {
            var ctx = new DiversityDataContext();
            return new LightList<Term>(from t in ctx.Terms
                                       where t.SourceID == source
                                       select t);
        }


        public IList<Event> getEventsForSeries(EventSeries es)
        {
            var ctx = new DiversityDataContext();
            return new LightList<Event>(from e in ctx.Events
                                        where e.SeriesID == es.SeriesID
                                        select e);
        }


        public IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec)
        {
            var ctx = new DiversityDataContext();
            return new LightList<IdentificationUnit>(from iu in ctx.IdentificationUnits
                                                     where iu.SpecimenID == spec.CollectionSpecimenID && iu.RelatedUnitID == null
                                                     select iu);
        }


        public IList<IdentificationUnit> getSubUnits(IdentificationUnit unit)
        {
            var ctx = new DiversityDataContext();
            return new LightList<IdentificationUnit>(from iu in ctx.IdentificationUnits
                                                     where iu.RelatedUnitID == unit.UnitID
                                                     select iu);
        }


        public void addIUnit(IdentificationUnit iu)
        {
            using (var ctx = new DiversityDataContext())
            {
                if (iu.IsModified == null)
                    iu.UnitID = findFreeUnitID(ctx);

                ctx.IdentificationUnits.InsertOnSubmit(iu);
                ctx.SubmitChanges();
            }
        }


        public void addSpecimen(Specimen spec)
        {
            using (var ctx = new DiversityDataContext())
            {
                if (spec.IsModified == null)
                    spec.CollectionSpecimenID = findFreeSpecimenID(ctx);

                ctx.Specimen.InsertOnSubmit(spec);
                ctx.SubmitChanges();
            }
        }

        private void sampleData()
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.EventSeries.InsertOnSubmit(new Model.EventSeries() { SeriesID = 0, Description = "ES" });
                ctx.Events.InsertOnSubmit(new Model.Event() { SeriesID = 0, EventID = 0, LocalityDescription = "EV" });
                ctx.Specimen.InsertOnSubmit(new Model.Specimen() { CollectionEventID = 0, CollectionSpecimenID = 0, AccesionNumber = "CS" });
                ctx.IdentificationUnits.InsertOnSubmit(new IdentificationUnit() { SpecimenID = 0, UnitDescription = "Top", UnitID = 0 });
                int id = 1;
                recSample(0, 0, ref id, ctx);
                ctx.SubmitChanges();
            }

        }
        private void recSample(int depth, int parent, ref int id, DiversityDataContext ctx)
        {
            if (depth == 3)
                return;

            depth++;
            int p = id;


            for (int i = 0; i < 20; i++)
            {
                ctx.IdentificationUnits.InsertOnSubmit(new IdentificationUnit() { UnitID = id, UnitDescription = "Sub" + id, RelatedUnitID = parent });
                recSample(depth, id++, ref id, ctx);
            }
        }


        public void addTaxonNames(IEnumerable<TaxonName> taxa)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.TaxonNames.InsertAllOnSubmit(taxa);
                ctx.SubmitChanges();
            }
        }

        public IList<TaxonName> getTaxonNames(Term taxonGroup)
        {
            var ctx = new DiversityDataContext();
            return new LightList<TaxonName>(from taxa in ctx.TaxonNames
                                            where taxa.TaxonomicGroup == taxonGroup.Code
                                            select taxa);
        }
    }
}
