
using System.Collections.Generic;
using Client = DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using DiversityPhone.DiversityService;
using System.Linq;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceObservableClient : IDiversityServiceClient
    {
        DiversityService.DiversityServiceClient _svc = new DiversityService.DiversityServiceClient();
        ISettingsService _settings;

        public DiversityServiceObservableClient(ISettingsService settings)
        {
            _settings = settings;
        }

        public UserCredentials GetCreds()
        {
            var settings = _settings.getSettings();
            return new UserCredentials(settings);            
        }

        private static IObservable<T> guardedSingleResultObservable<T>(IObservable<T> source)
        {
            var res = source
                .handleServiceExceptions()
                .FirstAsync()
                .Replay(1);

            res.Connect();
            return res;
        }


        public IObservable<UserProfile> GetUserInfo(UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetUserInfoCompletedEventArgs>, GetUserInfoCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetUserInfoCompleted += d, d => _svc.GetUserInfoCompleted -= d)
                .Select(args => args.Result);
            var res = guardedSingleResultObservable(source);
            _svc.GetUserInfoAsync(login);
            return res;
        }

        public IObservable<IList<Repository>> GetRepositories(DiversityService.UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetRepositoriesCompletedEventArgs>, GetRepositoriesCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetRepositoriesCompleted += d, d => _svc.GetRepositoriesCompleted -= d)
                .Select(args => args.Result as IList<Repository>);
            var res = guardedSingleResultObservable(source);
            _svc.GetRepositoriesAsync(login);
            return res;
        }

        public IObservable<IList<Project>> GetProjectsForUser(DiversityService.UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetProjectsForUserCompletedEventArgs>, GetProjectsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetProjectsForUserCompleted += d, d => _svc.GetProjectsForUserCompleted -= d)
                .Select(args => args.Result as IList<Project>);
            var res = guardedSingleResultObservable(source);
            _svc.GetProjectsForUserAsync(login);
            return res;
        }

        public IObservable<IEnumerable<TaxonList>> GetTaxonLists()
        {
            var source = Observable.FromEvent<EventHandler<GetTaxonListsForUserCompletedEventArgs>, GetTaxonListsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d)
                .Select(args => args.Result as IEnumerable<TaxonList>);
            var res = guardedSingleResultObservable(source);
            _svc.GetTaxonListsForUserAsync(GetCreds());
            return res;
        }

        public IObservable<IEnumerable<Client.TaxonName>> DownloadTaxonListChunked(TaxonList list)
        {
            var localclient = new DiversityServiceClient(); //Avoid race conditions from chunked download
            int chunk = 1; //First Chunk is 1, not 0!

            var res = Observable.FromEvent<EventHandler<DownloadTaxonListCompletedEventArgs>, DownloadTaxonListCompletedEventArgs>((a) => (s, args) => a(args), d => localclient.DownloadTaxonListCompleted += d, d => localclient.DownloadTaxonListCompleted -= d)                                
                .Select(args => args.Result ?? Enumerable.Empty<TaxonName>())
                .Select(taxa => taxa.Select(
                    taxon => new Client.TaxonName()
                    {
                        GenusOrSupragenic = taxon.GenusOrSupragenic,
                        InfraspecificEpithet = taxon.InfraspecificEpithet,
                        SpeciesEpithet = taxon.SpeciesEpithet,
                        Synonymy = (Client.Synonymy)Enum.Parse(typeof(Client.Synonymy),taxon.Synonymy,true),
                        TaxonNameCache = taxon.TaxonNameCache,
                        TaxonNameSinAuth = taxon.TaxonNameSinAuth,
                        URI = taxon.URI
                    }))
                .TakeWhile(taxonChunk => 
                    {
                        if(taxonChunk.Any())
                        {
                            //There might still be more Taxa -> request next chunk
                            localclient.DownloadTaxonListAsync(list, ++chunk, GetCreds());
                            return true;
                        }
                        else //Transfer finished
                            return false;
                    });
            //Request first chunk
            localclient.DownloadTaxonListAsync(list,chunk, GetCreds());
            return res;
        }

        public IObservable<IEnumerable<Client.Property>> GetPropertiesForUser(UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetPropertiesForUserCompletedEventArgs>, GetPropertiesForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetPropertiesForUserCompleted += d, d => _svc.GetPropertiesForUserCompleted -= d)
                .Select(args => args.Result
                    .Select(p => new Client.Property()
                    { 
                        PropertyID = p.PropertyID,                        
                        DisplayText = p.DisplayText
                    }));
            var res = guardedSingleResultObservable(source);
            _svc.GetPropertiesForUserAsync(login);
            return res;
        }


        public IObservable<IEnumerable<Client.PropertyName>> DownloadPropertyValuesChunked(Client.Property p)
        {            
            var localclient = new DiversityServiceClient(); //Avoid race conditions from chunked download
            var svcProperty = new Property()
            {
                PropertyID = p.PropertyID,               
                DisplayText = p.DisplayText
            };
            int chunk = 1; //First Chunk is 1, not 0!
            Func<IObservable<IEnumerable<Client.PropertyName>>> factory = () =>
                {
                    var obs = Observable.FromEvent<EventHandler<DownloadPropertyNamesCompletedEventArgs>, DownloadPropertyNamesCompletedEventArgs>((a) => (s, args) => a(args), d => localclient.DownloadPropertyNamesCompleted += d, d => localclient.DownloadPropertyNamesCompleted -= d)
                        .Select(args => args.Result ?? Enumerable.Empty<PropertyName>())
                        .Select(taxa => taxa.Select(
                            property => new Client.PropertyName
                            {
                                PropertyUri=property.PropertyUri,
                                PropertyID=property.PropertyID,                       
                                DisplayText = property.DisplayText,
                            }))
                        .Publish();
                    obs.Connect();
                    return obs;
                };


            var res = factory()
                .Catch((Exception ex) => 
                    {
                        var obs = factory();
                        localclient.DownloadPropertyNamesAsync(svcProperty, chunk, GetCreds()); // Re-Request last chunk
                        return obs;
                    })                
                .TakeWhile(taxonChunk =>
                {
                    if (taxonChunk.Any())
                    {
                        //There might still be more Taxa -> request next chunk
                        localclient.DownloadPropertyNamesAsync(svcProperty, ++chunk, GetCreds());
                        return true;
                    }
                    else //Transfer finished
                        return false;
                });
            //Request first chunk
            localclient.DownloadPropertyNamesAsync(svcProperty, chunk, GetCreds());
            return res;
        }

        public IObservable<IEnumerable<Client.Term>> GetStandardVocabulary()
        {
            var source = Observable.FromEvent<EventHandler<GetStandardVocabularyCompletedEventArgs>, GetStandardVocabularyCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetStandardVocabularyCompleted += d, d => _svc.GetStandardVocabularyCompleted -= d)
               .Select(args => args.Result)
               .Select(terms => terms
                   .Select(term => new Client.Term()
                   {
                       Code = term.Code,
                       Description = term.Description,
                       DisplayText = term.DisplayText,
                       ParentCode = term.ParentCode,
                       SourceID = term.Source
                   }));
            var res = guardedSingleResultObservable(source);
               
            _svc.GetStandardVocabularyAsync();
            return res;
        }

        public IObservable<IEnumerable<Client.Analysis>> GetAnalysesForProject(int projectID, UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetAnalysesForProjectCompletedEventArgs>, GetAnalysesForProjectCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetAnalysesForProjectCompleted += d, d => _svc.GetAnalysesForProjectCompleted -= d)
               .Select(args => args.Result)
               .Select(analyses => analyses
                   .Select(an => new Client.Analysis()
                   {
                       AnalysisID = an.AnalysisID,
                       Description = an.Description,
                       DisplayText = an.DisplayText,
                       MeasurementUnit = an.MeasurementUnit
                   }));
            var res = guardedSingleResultObservable(source);            
            _svc.GetAnalysesForProjectAsync(projectID, login);            
            return res;
        }

        public IObservable<IEnumerable<Client.AnalysisResult>> GetAnalysisResultsForProject(int projectID, UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetAnalysisResultsForProjectCompletedEventArgs>, GetAnalysisResultsForProjectCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetAnalysisResultsForProjectCompleted += d, d => _svc.GetAnalysisResultsForProjectCompleted -= d)
               .Select(args => args.Result)
               .Select(ars => ars
                   .Select(ar => new Client.AnalysisResult()
                   {
                       AnalysisID = ar.AnalysisID,
                       Description = ar.Description,
                       DisplayText = ar.DisplayText,
                       Notes = ar.Notes,
                       Result = ar.Result
                   }));
            var res = guardedSingleResultObservable(source);
            _svc.GetAnalysisResultsForProjectAsync(projectID, login);
            return res;
        }

        public IObservable<IEnumerable<Client.AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(int projectID, UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>, GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted += d, d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted -= d)
               .Select(args => args.Result)
               .Select(atgs => atgs
                   .Select(atg => new Client.AnalysisTaxonomicGroup()
                   {
                       AnalysisID = atg.AnalysisID,
                       TaxonomicGroup = atg.TaxonomicGroup
                   }));
            var res = guardedSingleResultObservable(source);

            _svc.GetAnalysisTaxonomicGroupsForProjectAsync(projectID, login);
            return res;
        }
    }
}
