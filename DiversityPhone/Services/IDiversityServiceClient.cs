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

        IObservable<IEnumerable<Analysis>> GetAnalysesForProject(Svc.Project p, Svc.UserCredentials login);

        IObservable<IEnumerable<AnalysisResult>> GetAnalysisResultsForProject(Svc.Project p, Svc.UserCredentials login);

        IObservable<IEnumerable<AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(Svc.Project p, Svc.UserCredentials login);

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

        IObservable<IEnumerable<Svc.PropertyList>> GetPropertyLists();

        IObservable<IEnumerable<PropertyName>> DownloadPropertyListChunked(Svc.PropertyList list);

        #endregion



    }
}