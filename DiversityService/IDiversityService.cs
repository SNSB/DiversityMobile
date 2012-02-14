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
        UserProfile GetUserInfo(UserCredentials login);

        [OperationContract]
        IEnumerable<Repository> GetRepositories(UserCredentials login);

        [OperationContract]
        IEnumerable<Project> GetProjectsForUser(UserCredentials login);

        //TODO Contract Userinfo

        [OperationContract]
        IEnumerable<TaxonList> GetTaxonListsForUser(UserProfile user);
        [OperationContract]
        IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page);

        [OperationContract]
        IEnumerable<Project> GetAvailableProjects();
        


        [OperationContract]
        IEnumerable<Term> GetStandardVocabulary();

        [OperationContract]
        String GetXMLStandardVocabulary();
        [OperationContract]
        IEnumerable<AnalysisResult> GetAnalysisResults(IList<int> analysisKeys);
        [OperationContract]
        IEnumerable<AnalysisTaxonomicGroup> GetAnalysisTaxonomicGroupsForProject(Project p);

        [OperationContract]
        IEnumerable<Model.Analysis> GetAnalysesForProject(Project p);
        [OperationContract]
        IEnumerable<Model.AnalysisResult> GetAnalysisResultsForProject(Project p);


        [OperationContract]
        IEnumerable<string> GetAvailablePropertyLists();
        [OperationContract]
        IEnumerable<PropertyName> DownloadPropertyList(string list);

        [OperationContract]
        Dictionary<EventSeries,EventSeries> InsertEventSeries(IList<EventSeries> series);


        [OperationContract]
        HierarchySection InsertHierarchy(HierarchySection hierarchy);
    }   
}
