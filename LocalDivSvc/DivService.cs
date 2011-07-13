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
            DiversityDataContext db = new DiversityDataContext();

            db.CollectionEvent.InsertOnSubmit(ev);
            
            db.SubmitChanges();
        }

        public IEnumerable<CollectionEvent> GetEvents(int skip, int count)
        {
            var repo = new DiversityDataContext();

            return repo.CollectionEvent.Skip(skip).Take(count).ToList();
        }


        public IEnumerable<CollectionSpecimen> GetSpecimensForEvent(CollectionEvent ev)
        {
            var repo = new DiversityDataContext();

            return from spec in repo.CollectionSpecimen
                   where spec.CollectionEventID == ev.CollectionEventID
                   select spec;
        }
    }

}
