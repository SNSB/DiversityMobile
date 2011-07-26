using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DiversityService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "DivService" sowohl im Code als auch in der Konfigurationsdatei ändern.
   public class DivService : IDivService
    {


        public void AddEvent(CollectionEvent ev)
        {
            var db = new DiversityEntities1();

            db.CollectionEvents.AddObject(ev);

            db.AcceptAllChanges();
        }

        public IEnumerable<CollectionEvent> GetEvents(int skip, int count)
        {
            var repo = new DiversityEntities1();

            return repo.CollectionEvents.Skip(skip).Take(count).ToList();
        }


        public IEnumerable<CollectionSpecimen> GetSpecimensForEvent(CollectionEvent ev)
        {
            var repo = new DiversityEntities1();

            return from spec in repo.CollectionSpecimen
                   where spec.CollectionEventID == ev.CollectionEventID
                   select spec;
        }


        public void InsertAnalysedIU(int EventID, IdentificationUnit owner, IList<IdentificationUnitAnalysis> analyses)
        {
            var db = new DiversityEntities1();

            var parentEvent = (from ev in db.CollectionEvents where ev.CollectionEventID == EventID select ev).FirstOrDefault();
            if (parentEvent != null)
            {
                var spec = new CollectionSpecimen() { RowGUID = Guid.NewGuid() };
                spec.CollectionAgents.Add(new CollectionAgent() { CollectorsName = "WP7User", RowGUID = Guid.NewGuid() });
                spec.CollectionProjects.Add(new CollectionProject() { ProjectID = 703, RowGUID = Guid.NewGuid() });
                spec.CollectionEvent = parentEvent;

            }
        }
    }

}
