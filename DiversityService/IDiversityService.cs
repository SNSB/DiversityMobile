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
        #region Repository
        #region Download
        [OperationContract]
        IEnumerable<Term> GetStandardVocabulary();

        [OperationContract]
        UserProfile GetUserInfo(UserCredentials login);

        [OperationContract]
        IEnumerable<Repository> GetRepositories(UserCredentials login);

        [OperationContract]
        IEnumerable<Project> GetProjectsForUser(UserCredentials login);
        
        [OperationContract]
        IEnumerable<AnalysisTaxonomicGroup> GetAnalysisTaxonomicGroupsForProject(Project p, UserCredentials login);
        [OperationContract]
        IEnumerable<Analysis> GetAnalysesForProject(Project p, UserCredentials login);
        [OperationContract]
        IEnumerable<AnalysisResult> GetAnalysisResultsForProject(Project p, UserCredentials login);
        #endregion

        #region Upload

        [OperationContract]
        Dictionary<int, int> InsertEventSeries(IList<EventSeries> series, UserCredentials login);
        [OperationContract]
        void InsertGeographyIntoSeries(int seriesID, string geostring, UserCredentials login);

        [OperationContract]
        KeyProjection InsertHierarchy(HierarchySection hierarchy, UserCredentials cred);

        #endregion
        #endregion

        #region DB "DiversityMobile" Attribute SNSB

        [OperationContract]
        IEnumerable<TaxonList> GetTaxonListsForUser(UserCredentials login);
        [OperationContract]
        IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page, UserCredentials login);
        [OperationContract]
        IEnumerable<PropertyList> GetPropertyListsForUser(UserCredentials login);
        [OperationContract]
        IEnumerable<PropertyName> DownloadPropertyList(PropertyList list, int page, UserCredentials login);

        #endregion


       

        #region Android
        
        [OperationContract]
        String GetXMLStandardVocabulary();

        [OperationContract]
        int InsertEventSeriesForAndroid(int SeriesID, String Description);

        #endregion;
    }   
}
