using DiversityPhone.DiversityService;
using System;
using System.Collections.Generic;
namespace DiversityPhone.Services
{
    public interface IDiversityServiceClient 
    {        
        IObservable<UserProfile> GetUserInfo(UserCredentials login);
        
        IObservable<IList<Repository>> GetRepositories(UserCredentials login);
        
        IObservable<IList<Project>> GetProjectsForUser(UserCredentials login);
      
        IObservable<IEnumerable<TaxonList>> GetTaxonLists();
       
        IObservable<IEnumerable<TaxonName>> DownloadTaxonListChunked(TaxonList list);
       
        IObservable<IEnumerable<Term>> GetStandardVocabulary();    
    }
}