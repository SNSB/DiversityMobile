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

       

        IQueryable<EventSeries> IOfflineFieldData.EventSeries
        {
            get { throw new NotImplementedException(); }
        }

        public IList<EventSeries> getAllEventSeries()
        {
            var ctx = new DiversityDataContext();
            return new LightList<EventSeries>(ctx.EventSeries);
        }

        private int findFreeEventSeriesID(DiversityDataContext ctx)
        {
            int min = -1;
            if(ctx.EventSeries.Any())
               min = (from es in ctx.EventSeries select es.SeriesID).Min();
            return (min > -1)? -1 : min - 1;
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
            throw new NotImplementedException();
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
    }
}
