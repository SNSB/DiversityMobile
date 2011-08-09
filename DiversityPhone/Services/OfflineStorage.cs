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

namespace DiversityPhone.Services
{
    public class OfflineStorage : IOfflineStorage
    {
        private ISterlingDatabaseInstance _db { get; set; }
        public OfflineStorage()
        {
            var context = new DiversityDataContext();
            if (!context.DatabaseExists())
                context.CreateDatabase();
            context.EventSeries.InsertOnSubmit(new global::DiversityService.Model.EventSeries() { SeriesID = 1234 });
            context.SubmitChanges();
            var count = context.EventSeries.Count();
        }


        public System.Collections.Generic.IList<global::DiversityService.Model.EventSeries> EventSeries
        {
            get { throw new NotImplementedException(); }
        }


        public System.Collections.Generic.IList<global::DiversityService.Model.EventSeries> getEventSeriesByDescription(string query)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<global::DiversityService.Model.EventSeries> getNewEventSeries()
        {
            throw new NotImplementedException();
        }

        public global::DiversityService.Model.EventSeries getEventSeriesByID(int id)
        {
            throw new NotImplementedException();
        }

        public void addEventSeries(global::DiversityService.Model.EventSeries newSeries)
        {
            throw new NotImplementedException();
        }
    }
}
