using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DiversityService.Model;
using DiversityPhone.Model;

namespace DiversityService
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IDivService" sowohl im Code als auch in der Konfigurationsdatei ändern.    
    [ServiceContract]
    public interface IDiversityService
    {
        #region Repository
        #region Vocabulary
        [OperationContract]
        IEnumerable<Term> GetStandardVocabulary(UserCredentials login);

        [OperationContract]
        UserProfile GetUserInfo(UserCredentials login);

        [OperationContract]
        IEnumerable<Repository> GetRepositories(UserCredentials login);

        [OperationContract]
        IEnumerable<Qualification> GetQualifications(UserCredentials login);

        [OperationContract]
        IEnumerable<Project> GetProjectsForUser(UserCredentials login);
        
        [OperationContract]
        IEnumerable<AnalysisTaxonomicGroup> GetAnalysisTaxonomicGroupsForProject(int projectID, UserCredentials login);
        [OperationContract]
        IEnumerable<Analysis> GetAnalysesForProject(int projectID, UserCredentials login);
        [OperationContract]
        IEnumerable<AnalysisResult> GetAnalysisResultsForProject(int projectID, UserCredentials login);
        #endregion

        #region Upload

        [OperationContract]
        int InsertEventSeries(EventSeries series, IEnumerable<Localization> localizations, UserCredentials login);

        [OperationContract]
        int InsertEvent(Event ev, IEnumerable<EventProperty> properties, UserCredentials login);

        [OperationContract]
        int InsertSpecimen(Specimen s, UserCredentials login);

        [OperationContract]
        int InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses, UserCredentials login);

        [OperationContract]
        void InsertMMO(MultimediaObject mmo, UserCredentials cred);

        #endregion

        #region Download
        [OperationContract]
        EventSeries EventSeriesByID(int collectionSeriesID, UserCredentials login);

        [OperationContract]
        IEnumerable<Localization> LocalizationsForSeries(int collectionSeriesID, UserCredentials login);

        [OperationContract]
        IEnumerable<Event> EventsByLocality(String locality, UserCredentials login);

        [OperationContract]
        IEnumerable<EventProperty> PropertiesForEvent(int collectionEventID, UserCredentials login);

        [OperationContract]
        IEnumerable<Specimen> SpecimenForEvent(int collectionEventID, UserCredentials login);

        [OperationContract]
        IEnumerable<IdentificationUnit> UnitsForSpecimen(int collectionSpecimenID, UserCredentials login);

        [OperationContract]
        IEnumerable<IdentificationUnit> SubUnitsForIU(int collectionUnitID, UserCredentials login);

        [OperationContract]
        IEnumerable<IdentificationUnitAnalysis> AnalysesForIU(int collectionUnitID, UserCredentials login);
        #endregion

        #endregion

        #region DB "DiversityMobile" Attribute SNSB

        [OperationContract]
        IEnumerable<TaxonList> GetTaxonListsForUser(UserCredentials login);       
        [OperationContract]
        IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page, UserCredentials login);
        [OperationContract]
        IEnumerable<Property> GetPropertiesForUser(UserCredentials login);
        [OperationContract]
        IEnumerable<PropertyValue> DownloadPropertyNames(Property p, int page, UserCredentials login);

        #endregion

    }   
}
