using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ConsoleApplication1
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "DiversityService" sowohl im Code als auch in der Konfigurationsdatei ändern.
    public class DiversityService : IDiversityService
    {


        public void AddEvent(CollectionEvent ev)
        {
            DiversityDataContext db = new DiversityDataContext();
            db.CollectionEvent.InsertOnSubmit(ev);
            db.SubmitChanges();
        }

        public IEnumerable<CollectionEvent> GetEvents(int skip, int count)
        {
            DiversityDataContext db = new DiversityDataContext();
            return (from ev in db.CollectionEvent select ev).Skip(skip).Take(count);
        }
    }
}
