using System;
using System.Collections.Generic;
using Svc = DiversityPhone.DiversityService;
using DiversityPhone.Model;
namespace DiversityPhone.Services
{
    public interface IDiversityServiceClient 
    {        
        IObservable<Svc.UserProfile> GetUserInfo(Svc.UserCredentials login);

        IObservable<IList<Svc.Repository>> GetRepositories(Svc.UserCredentials login);

        IObservable<IList<Svc.Project>> GetProjectsForUser(Svc.UserCredentials login);

        IObservable<IEnumerable<Svc.TaxonList>> GetTaxonLists();

        IObservable<IEnumerable<TaxonName>> DownloadTaxonListChunked(Svc.TaxonList list);
       
        IObservable<IEnumerable<Term>> GetStandardVocabulary();

        IObservable<Svc.KeyProjection> InsertHierarchy(Svc.HierarchySection section);

        IObservable<Dictionary<int, int>> InsertEventSeries(IEnumerable<EventSeries> seriesList);

    }
}