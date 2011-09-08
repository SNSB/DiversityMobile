using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Wintellect.Sterling;
using System.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using DiversityPhone.Model;
using System.Data.Linq;

namespace DiversityPhone.Services
{
    public class OfflineStorage : IOfflineStorage
    {
        public OfflineStorage()
        {
            var context = new DiversityDataContext();
            if (!context.DatabaseExists())
                context.CreateDatabase();

            
        }

        public void addEventSeries(global::DiversityPhone.Model.EventSeries newSeries)
        {            
            if (newSeries.IsModified != null || newSeries.SeriesID != default(int))
                throw new InvalidOperationException("Series is not new!");

            var ctx = new DiversityDataContext();
            newSeries.SeriesID = findFreeEventSeriesID(ctx);
            ctx.EventSeries.InsertOnSubmit(newSeries);
            ctx.SubmitChanges();
        }     

      

        public IList<EventSeries> getAllEventSeries()
        {
            var ctx = new DiversityDataContext();
            return new LightList<EventSeries>(ctx.EventSeries);
        }

        private static int findFreeEventSeriesID(DiversityDataContext ctx)
        {
            int min = -1;
            if(ctx.EventSeries.Any())
               min = (from es in ctx.EventSeries select es.SeriesID).Min();
            return (min > -1)? -1 : min - 1;
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
            var ctx = new DiversityDataContext();
            ctx.Events.InsertOnSubmit(ev);
            ctx.SubmitChanges();
        }

        public void addTerms(IEnumerable<Term> terms)
        {
            var ctx = new DiversityDataContext();
            ctx.Terms.InsertAllOnSubmit(terms);
            ctx.SubmitChanges();
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


        public IList<IdentificationUnit> getTopLevelIUForEvent(Event ev)
        {
            var ctx = new DiversityDataContext();
            return new LightList<IdentificationUnit>(from iu in ctx.IdentificationUnits
                                                    where iu.EventID == ev.EventID && iu.RelatedUnitID == null
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
            var ctx = new DiversityDataContext();
            if (iu.IsModified == null)
                iu.UnitID = findFreeUnitID(ctx);
            
            ctx.IdentificationUnits.InsertOnSubmit(iu);
            ctx.SubmitChanges();
        }
    }
}
