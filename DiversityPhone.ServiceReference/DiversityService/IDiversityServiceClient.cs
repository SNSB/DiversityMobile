using System;
using System.Collections.Generic;
using Svc = DiversityPhone.DiversityService;
using DiversityPhone.Model;
using System.Reactive;
namespace DiversityPhone.Services
{
    public interface IDiversityServiceClient
    {
        #region Repository

        #region Download
        IObservable<Svc.UserProfile> GetUserInfo(Svc.UserCredentials login);

        IObservable<IList<Svc.Repository>> GetRepositories(Svc.UserCredentials login);

        IObservable<IList<Svc.Project>> GetProjectsForUser(Svc.UserCredentials login);
       
        IObservable<IEnumerable<Term>> GetStandardVocabulary();

        IObservable<IEnumerable<Analysis>> GetAnalysesForProject(int projectID, Svc.UserCredentials login);

        IObservable<IEnumerable<AnalysisResult>> GetAnalysisResultsForProject(int projectID, Svc.UserCredentials login);

        IObservable<IEnumerable<AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(int projectID, Svc.UserCredentials login);

        IObservable<IEnumerable<Qualification>> GetQualifications(Svc.UserCredentials credentials);
        #endregion
        #region Upload        

        IObservable<Unit> InsertEventSeries(EventSeries series, IEnumerable<ILocalizable> localizations);
        IObservable<Unit> InsertEvent(Event ev, IEnumerable<EventProperty> properties);
        IObservable<Unit> InsertSpecimen(Specimen spec);
        IObservable<Unit> InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses);

        IObservable<Unit> InsertMultimediaObject(MultimediaObject mmo);        
        #endregion

        IObservable<String> UploadMultimedia(MultimediaObject Uri, byte[] data);

        #endregion

        #region DB "DiversityMobile" at SNSB 
        IObservable<IEnumerable<TaxonList>> GetTaxonLists();

        IObservable<IEnumerable<TaxonName>> DownloadTaxonListChunked(TaxonList list);

        IObservable<IEnumerable<Property>> GetPropertiesForUser(Svc.UserCredentials login);

        IObservable<IEnumerable<PropertyName>> DownloadPropertyValuesChunked(Property p);

        #endregion




        
    }
}