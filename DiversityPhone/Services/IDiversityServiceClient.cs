using System;
using System.Collections.Generic;
using Svc = DiversityPhone.DiversityService;
using DiversityPhone.Model;
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

        #endregion
        #region Upload
        IObservable<Svc.KeyProjection> InsertHierarchy(Svc.HierarchySection section);

        IObservable<Dictionary<int, int>> InsertEventSeries(IEnumerable<Svc.EventSeries> seriesList);
        Svc.UserCredentials GetCreds();
        #endregion

        #endregion

        #region DB "DiversityMobile" at SNSB 
        IObservable<IEnumerable<Svc.TaxonList>> GetTaxonLists();

        IObservable<IEnumerable<TaxonName>> DownloadTaxonListChunked(Svc.TaxonList list);

        IObservable<IEnumerable<Property>> GetPropertiesForUser();

        IObservable<IEnumerable<PropertyName>> DownloadPropertyValuesChunked(Property p);

        #endregion



    }
}