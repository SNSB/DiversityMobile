﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DiversityService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IService1" sowohl im Code als auch in der Konfigurationsdatei ändern.
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
