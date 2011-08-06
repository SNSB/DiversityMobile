using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DiversityService.Model;

namespace DiversityService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IDivService" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IDiversityService
    {
        [OperationContract]
        IList<Project> GetProjectsForUser(UserProfile user);


        [OperationContract]
        IList<EventSeries> GetSeriesByDescription(string description);

        [OperationContract]
        IList<EventSeries> AllEventSeries();



    }   
}
