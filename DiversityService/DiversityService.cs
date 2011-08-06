using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService
{
    public class DiversityService : IDiversityService
    {
        //public HierarchySection GetSectionForSeries(int seriesID);
        //public HierarchySection GetSectionForEvent(int eventID);
        //public HierarchySection GetSectionForIU(int iuID);

        

        


        //public void InsertSection(HierarchySection section)
        //{
        //    var db = new DiversityCollection_BaseTestEntities();


        //    var parentEvent = (from ev in db.CollectionEvent where ev.CollectionEventID == EventID select ev).FirstOrDefault();
        //    if (parentEvent != null)
        //    {
        //        var spec = new CollectionSpecimen
        //        {
        //            RowGUID = Guid.NewGuid(),
        //            CollectionEvent = parentEvent
        //        };
        //        var agent = new CollectionAgent
        //        {
        //            CollectorsName = "WP7User",
        //            RowGUID = Guid.NewGuid(),
        //            CollectionSpecimen = spec
        //        };
        //        var project = new CollectionProject()
        //        {
        //            ProjectID = 703,
        //            RowGUID = Guid.NewGuid(),
        //            CollectionSpecimen = spec
        //        };
        //        child.CollectionSpecimen = spec;

        //        db.SaveChanges();
        //    }
        //}

        public IList<SimpleModel.EventSeries> GetSeriesByDescription(string description)
        {
            throw new NotImplementedException();
        }
    }
}
