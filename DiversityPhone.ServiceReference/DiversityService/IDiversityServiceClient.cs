using System;
using System.Collections.Generic;
using Svc = DiversityPhone.DiversityService;
using DiversityPhone.Model;
using System.Reactive;
namespace DiversityPhone.Services
{
    public interface IDiversityServiceClient
    {
        #region Vocabulary
        IObservable<Svc.UserProfile> GetUserInfo(Svc.UserCredentials login);

        IObservable<IList<Svc.Repository>> GetRepositories(Svc.UserCredentials login);

        IObservable<IList<Svc.Project>> GetProjectsForUser(Svc.UserCredentials login);

        IObservable<IEnumerable<Term>> GetStandardVocabulary();

        IObservable<IEnumerable<Analysis>> GetAnalysesForProject(int projectID, Svc.UserCredentials login);

        IObservable<IEnumerable<AnalysisResult>> GetAnalysisResultsForProject(int projectID, Svc.UserCredentials login);

        IObservable<IEnumerable<AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(int projectID, Svc.UserCredentials login);

        IObservable<IEnumerable<Qualification>> GetQualifications(Svc.UserCredentials credentials);

        IObservable<IEnumerable<TaxonList>> GetTaxonLists();

        IObservable<IEnumerable<TaxonName>> DownloadTaxonListChunked(TaxonList list);

        IObservable<IEnumerable<Property>> GetPropertiesForUser(Svc.UserCredentials login);

        IObservable<IEnumerable<PropertyName>> DownloadPropertyValuesChunked(Property p);
        #endregion

        #region Download
        IObservable<EventSeries> GetEventSeriesByID(int seriesID);
        IObservable<IEnumerable<Localization>> GetEventSeriesLocalizations(int seriesID);

        IObservable<IEnumerable<Event>> GetEventsByLocality(string localityQuery);
        IObservable<IEnumerable<EventProperty>> GetEventProperties(int eventID);        

        IObservable<IEnumerable<Specimen>> GetSpecimenForEvent(int eventID);

        IObservable<IEnumerable<IdentificationUnit>> GetIdentificationUnitsForSpecimen(int specimenID);
        IObservable<IEnumerable<IdentificationUnit>> GetSubUnitsForIU(int unitID);
        IObservable<IEnumerable<IdentificationUnitAnalysis>> GetAnalysesForIU(int unitID);

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