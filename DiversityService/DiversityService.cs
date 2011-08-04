using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService
{
    public class DiversityService : IDiversityService
    {


        public void AddEvent(CollectionEvent ev)
        {
            var db = new DiversityCollection_BaseTestEntities();

            db.CollectionEvent.AddObject(ev);

            db.AcceptAllChanges();
        }

        public IEnumerable<CollectionEvent> GetEvents(int skip, int count)
        {
            var repo = new DiversityCollection_BaseTestEntities();

            return repo.CollectionEvent.Skip(skip).Take(count).ToList();
        }


        public IEnumerable<CollectionSpecimen> GetSpecimensForEvent(CollectionEvent ev)
        {
            var repo = new DiversityCollection_BaseTestEntities();

            return from spec in repo.CollectionSpecimen
                   where spec.CollectionEventID == ev.CollectionEventID
                   select spec;
        }


        public void InsertIU(int EventID, IdentificationUnit child)
        {
            var db = new DiversityCollection_BaseTestEntities();


            var parentEvent = (from ev in db.CollectionEvent where ev.CollectionEventID == EventID select ev).FirstOrDefault();
            if (parentEvent != null)
            {
                var spec = new CollectionSpecimen
                {
                    RowGUID = Guid.NewGuid(),
                    CollectionEvent = parentEvent
                };
                var agent = new CollectionAgent
                {
                    CollectorsName = "WP7User",
                    RowGUID = Guid.NewGuid(),
                    CollectionSpecimen = spec
                };
                var project = new CollectionProject()
                {
                    ProjectID = 703,
                    RowGUID = Guid.NewGuid(),
                    CollectionSpecimen = spec
                };
                child.CollectionSpecimen = spec;

                db.SaveChanges();
            }
        }
    }
}
