using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;

namespace DiversityPhone.Interface
{
    public class ServiceNotAvailableException : Exception
    {
        public ServiceNotAvailableException()
        {
        }

        public ServiceNotAvailableException(string message)
            : base(message)
        {
        }

        public ServiceNotAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ServiceOperationException : Exception
    {
        public ServiceOperationException()
        {
        }

        public ServiceOperationException(string message)
            : base(message)
        {
        }

        public ServiceOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

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

        #endregion Vocabulary

        #region Download

        IObservable<IEnumerable<EventSeries>> GetEventSeriesByQuery(string query);

        IObservable<EventSeries> GetEventSeriesByID(int collectionSeriesID);

        IObservable<IEnumerable<Localization>> GetEventSeriesLocalizations(int collectionSeriesID);

        IObservable<IEnumerable<Event>> GetEventSeriesEvents(int collectionSeriesID);

        IObservable<IEnumerable<Event>> GetEventsByLocality(string localityQuery);

        IObservable<IEnumerable<EventProperty>> GetEventProperties(int collectionEventID);

        IObservable<IEnumerable<Specimen>> GetSpecimenForEvent(int collectionEventID);

        IObservable<IEnumerable<IdentificationUnit>> GetIdentificationUnitsForSpecimen(int collectionSpecimenID);

        IObservable<IEnumerable<IdentificationUnit>> GetSubUnitsForIU(int collectionUnitID);

        IObservable<IEnumerable<IdentificationUnitAnalysis>> GetAnalysesForIU(int collectionUnitID);

        #endregion Download

        #region Upload

        IObservable<Unit> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations);

        IObservable<Unit> InsertEvent(Event ev, IEnumerable<EventProperty> properties);

        IObservable<Unit> InsertSpecimen(Specimen spec);

        IObservable<Unit> InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses);

        IObservable<Unit> InsertMultimediaObject(MultimediaObject mmo);

        IObservable<Uri> UploadMultimedia(MultimediaObject Owner, Stream data);

        #endregion Upload
    }
}