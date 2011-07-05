using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DiversityService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als auch in der SVC- und der Konfigurationsdatei ändern.
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
            
                return Enumerable.Empty<CollectionEvent>();           
           
        }
    }
}
