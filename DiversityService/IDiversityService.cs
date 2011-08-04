using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace DiversityService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IDivService" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IDiversityService
    {

        [OperationContract]
        IEnumerable<CollectionEvent> GetEvents(int skip, int count);

        [OperationContract]
        void AddEvent(CollectionEvent ev);

        [OperationContract]
        IEnumerable<CollectionSpecimen> GetSpecimensForEvent(CollectionEvent ev);

        [OperationContract]
        void InsertIU(int CollectionEventID, IdentificationUnit child);


    }   
}
