using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ConsoleApplication1
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IDiversityService" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IDiversityService
    {

        [OperationContract]
        IEnumerable<CollectionEvent> GetEvents(int skip, int count);

        [OperationContract]
        void AddEvent(CollectionEvent ev);

        // TODO: Hier Dienstvorgänge hinzufügen
    }   

}
