
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

        private static IObservable<T> singleResultObservable<T>(IObservable<T> source)
        {
            var res = source
                .FirstAsync()
                .Replay(1);

            res.Connect();
            return res;
        }


        public IObservable<UserProfile> GetUserInfo(UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetUserInfoCompletedEventArgs>, GetUserInfoCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetUserInfoCompleted += d, d => _svc.GetUserInfoCompleted -= d)
                .Select(args => args.Result);
            var res = singleResultObservable(source);
            _svc.GetUserInfoAsync(login);
            return res;
        }

        public IObservable<IList<Repository>> GetRepositories(DiversityService.UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetRepositoriesCompletedEventArgs>, GetRepositoriesCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetRepositoriesCompleted += d, d => _svc.GetRepositoriesCompleted -= d)
                .Select(args => args.Result as IList<Repository>);
            var res = singleResultObservable(source);
            _svc.GetRepositoriesAsync(login);
            return res;
        }

        public IObservable<IList<Project>> GetProjectsForUser(DiversityService.UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetProjectsForUserCompletedEventArgs>, GetProjectsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetProjectsForUserCompleted += d, d => _svc.GetProjectsForUserCompleted -= d)
                .Select(args => args.Result as IList<Project>);
            var res = singleResultObservable(source);
            _svc.GetProjectsForUserAsync(login);
            return res;
        }

        public IObservable<IEnumerable<TaxonList>> GetTaxonLists()
        {
            var source = Observable.FromEvent<EventHandler<GetTaxonListsForUserCompletedEventArgs>, GetTaxonListsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d)
                .Select(args => args.Result as IEnumerable<TaxonList>);
            var res = singleResultObservable(source);
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

        public IObservable<IEnumerable<PropertyList>> GetPropertyLists()
        {
            var source = Observable.FromEvent<EventHandler<GetPropertyListsForUserCompletedEventArgs>, GetPropertyListsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetPropertyListsForUserCompleted += d, d => _svc.GetPropertyListsForUserCompleted -= d)
                .Select(args => args.Result as IEnumerable<PropertyList>);
            var res = singleResultObservable(source);
            _svc.GetPropertyListsForUserAsync(GetCreds());
            return res;
        }


        public IObservable<IEnumerable<Client.PropertyName>> DownloadPropertyListChunked(PropertyList list)
        {
            var localclient = new DiversityServiceClient(); //Avoid race conditions from chunked download
            int chunk = 1; //First Chunk is 1, not 0!

            var res = Observable.FromEvent<EventHandler<DownloadPropertyListCompletedEventArgs>, DownloadPropertyListCompletedEventArgs>((a) => (s, args) => a(args), d => localclient.DownloadPropertyListCompleted += d, d => localclient.DownloadPropertyListCompleted -= d)
                .Select(args => args.Result ?? Enumerable.Empty<PropertyName>())
                .Select(taxa => taxa.Select(
                    property => new Client.PropertyName
                    {
                        PropertyUri=property.PropertyUri,
                        PropertyID=property.PropertyID,
                        TermID=property.TermID,
                        BroaderTermID = property.BroaderTermID,
                        DisplayText = property.DisplayText,

                    }))
                .TakeWhile(taxonChunk =>
                {
                    if (taxonChunk.Any())
                    {
                        //There might still be more Taxa -> request next chunk
                        localclient.DownloadPropertyListAsync(list, ++chunk, GetCreds());
                        return true;
                    }
                    else //Transfer finished
                        return false;
                });
            //Request first chunk
            localclient.DownloadPropertyListAsync(list, chunk, GetCreds());
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
            var res = singleResultObservable(source);
               
            _svc.GetStandardVocabularyAsync();
            return res;
        }

        public IObservable<IEnumerable<Client.Analysis>> GetAnalysesForProject(Project p, UserCredentials login)
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
            var res = singleResultObservable(source);            
            _svc.GetAnalysesForProjectAsync(p, login);            
            return res;
        }

        public IObservable<IEnumerable<Client.AnalysisResult>> GetAnalysisResultsForProject(Project p, UserCredentials login)
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
            var res = singleResultObservable(source);
            _svc.GetAnalysisResultsForProjectAsync(p, login);
            return res;
        }

        public IObservable<IEnumerable<Client.AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(Project p, UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>, GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted += d, d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted -= d)
               .Select(args => args.Result)
               .Select(atgs => atgs
                   .Select(atg => new Client.AnalysisTaxonomicGroup()
                   {
                       AnalysisID = atg.AnalysisID,
                       TaxonomicGroup = atg.TaxonomicGroup
                   }));
            var res = singleResultObservable(source);
               
            _svc.GetAnalysisTaxonomicGroupsForProjectAsync(p, login);
            return res;
        }
    }
}
