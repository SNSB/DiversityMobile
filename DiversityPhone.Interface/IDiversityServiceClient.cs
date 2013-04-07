using System;
using System.Collections.Generic;
using DiversityPhone.Model;
using System.Reactive;
namespace DiversityPhone.Interface
{
    public interface IDiversityServiceClient
    {
        #region Vocabulary
        IObservable<UserProfile> GetUserInfo(UserCredentials login);

        IObservable<IEnumerable<string>> GetRepositories(UserCredentials login);

        IObservable<IList<Project>> GetProjectsForUser(UserCredentials login);

        IObservable<IEnumerable<Term>> GetStandardVocabulary();

        IObservable<IEnumerable<Analysis>> GetAnalysesForProject(int projectID, UserCredentials login);

        IObservable<IEnumerable<AnalysisResult>> GetAnalysisResultsForProject(int projectID, UserCredentials login);

        IObservable<IEnumerable<AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(int projectID, UserCredentials login);

        IObservable<IEnumerable<Qualification>> GetQualifications(UserCredentials credentials);

        IObservable<IEnumerable<TaxonList>> GetTaxonLists();

        IObservable<IEnumerable<TaxonName>> DownloadTaxonListChunked(TaxonList list);

        IObservable<IEnumerable<Property>> GetPropertiesForUser(UserCredentials login);

        IObservable<IEnumerable<PropertyName>> DownloadPropertyValuesChunked(Property p);
        #endregion

        #region Download
        IObservable<EventSeries> GetEventSeriesByID(int collectionSeriesID);
        IObservable<IEnumerable<Localization>> GetEventSeriesLocalizations(int collectionSeriesID);

        IObservable<IEnumerable<Event>> GetEventsByLocality(string localityQuery);
        IObservable<IEnumerable<EventProperty>> GetEventProperties(int collectionEventID);

        IObservable<IEnumerable<Specimen>> GetSpecimenForEvent(int collectionEventID);

        IObservable<IEnumerable<IdentificationUnit>> GetIdentificationUnitsForSpecimen(int collectionSpecimenID);
        IObservable<IEnumerable<IdentificationUnit>> GetSubUnitsForIU(int collectionUnitID);
        IObservable<IEnumerable<IdentificationUnitAnalysis>> GetAnalysesForIU(int collectionUnitID);

        #endregion

        #region Upload

        IObservable<Unit> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations);
        IObservable<Unit> InsertEvent(Event ev, IEnumerable<EventProperty> properties);
        IObservable<Unit> InsertSpecimen(Specimen spec);
        IObservable<Unit> InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses);

        IObservable<Unit> InsertMultimediaObject(MultimediaObject mmo);

        IObservable<String> UploadMultimedia(MultimediaObject Uri, byte[] data);
        #endregion











    }
}