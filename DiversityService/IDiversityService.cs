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
        

        [OperationContract]
        IEnumerable<TaxonList> GetTaxonListsForUser(UserCredentials login);
        [OperationContract]
        IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page);


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
        void InsertGeographyIntoSeries(int seriesID, string geostring);

        [OperationContract]
        KeyProjection InsertHierarchy(HierarchySection hierarchy);

        [OperationContract]
        HierarchySection CreateNewHierarchy();
    }   
}
