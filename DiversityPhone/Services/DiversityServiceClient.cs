using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reactive.Linq;
using System.Linq;
using DiversityPhone.DiversityService;
using System.Reactive.Subjects;
using System.Collections;
using System.Collections.Generic;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public class DiversityServiceClient : IDiversityServiceClient
    {
        DiversityService.DiversityServiceClient _svc = new DiversityService.DiversityServiceClient();
        ISettingsService _settings;

        public DiversityServiceClient(ISettingsService settings)
        {
            _settings = settings;
        }

        private UserCredentials GetCreds()
        {
            var settings = _settings.getSettings();
            return new UserCredentials()
            {
                LoginName = settings.UserName,
                AgentName=settings.AgentName,
                AgentURI=settings.AgentURI,
                ProjectID=settings.CurrentProject,
                Password = settings.Password,
                Repository = settings.HomeDB
            };
        }


        public IObservable<DiversityService.UserProfile> GetUserInfo(DiversityService.UserCredentials login)
        {
            var res = Observable.FromEvent<EventHandler<GetUserInfoCompletedEventArgs>, GetUserInfoCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetUserInfoCompleted += d, d => _svc.GetUserInfoCompleted -= d)
                .Select(args => args.Result)
                .Take(1);
            _svc.GetUserInfoAsync(login);
            return res;
        }

        public IObservable<IList<Repository>> GetRepositories(DiversityService.UserCredentials login)
        {
            var res = Observable.FromEvent<EventHandler<GetRepositoriesCompletedEventArgs>, GetRepositoriesCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetRepositoriesCompleted += d, d => _svc.GetRepositoriesCompleted -= d)
                .Select(args => args.Result as IList<Repository>)                
                .Take(1);
            _svc.GetRepositoriesAsync(login);
            return res;
        }

        public IObservable<IList<Project>> GetProjectsForUser(DiversityService.UserCredentials login)
        {
            var res = Observable.FromEvent<EventHandler<GetProjectsForUserCompletedEventArgs>, GetProjectsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetProjectsForUserCompleted += d, d => _svc.GetProjectsForUserCompleted -= d)
                .Select(args => args.Result as IList<Project>)
                .Take(1);
            _svc.GetProjectsForUserAsync(login);
            return res;
        }

        public IObservable<IEnumerable<TaxonList>> GetTaxonLists()
        {            
            var res = Observable.FromEvent<EventHandler<GetTaxonListsForUserCompletedEventArgs>, GetTaxonListsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d)
                .Select(args => args.Result as IEnumerable<TaxonList>)
                .Take(1);
            _svc.GetTaxonListsForUserAsync(GetCreds());
            return res;
        }

        public IObservable<IEnumerable<TaxonName>> DownloadTaxonListChunked(TaxonList list)
        {
            var localclient = new Svc.DiversityServiceClient(); //Avoid race conditions from chunked download
            int chunk = 1; //First Chunk is 1, not 0!

            var res = Observable.FromEvent<EventHandler<DownloadTaxonListCompletedEventArgs>, DownloadTaxonListCompletedEventArgs>((a) => (s, args) => a(args), d => localclient.DownloadTaxonListCompleted += d, d => localclient.DownloadTaxonListCompleted -= d)                
                .Select(args => args.Result as IEnumerable<TaxonName>)
                .TakeWhile(taxonChunk => 
                    {
                        if(taxonChunk.Any())
                        {
                            //There might still be more Taxa -> request next chunk
                            localclient.DownloadTaxonListAsync(list, ++chunk);
                            return true;
                        }
                        else //Transfer finished
                            return false;
                    });
            //Request first chunk
            localclient.DownloadTaxonListAsync(list,chunk);
            return res;
        }

        public IObservable<IEnumerable<Term>> GetStandardVocabulary()
        {
            var res = Observable.FromEvent<EventHandler<GetStandardVocabularyCompletedEventArgs>, GetStandardVocabularyCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetStandardVocabularyCompleted += d, d => _svc.GetStandardVocabularyCompleted -= d)
               .Select(args => args.Result as IEnumerable<Term>)
               .Take(1);
            _svc.GetStandardVocabularyAsync();
            return res;
        }

        public IObservable<Svc.KeyProjection> InsertHierarchy(Svc.HierarchySection section)
        {
            var res = Observable.FromEvent<EventHandler<InsertHierarchyCompletedEventArgs>, InsertHierarchyCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.InsertHierarchyCompleted += d, d => _svc.InsertHierarchyCompleted -= d)
                .Select(args => args.Result)
                .Take(1);
            _svc.InsertHierarchyAsync(section, this.GetCreds());
            return res;
        }



        public IObservable<Dictionary<int, int>> InsertEventSeries(System.Collections.ObjectModel.ObservableCollection<EventSeries> seriesList)
        {
            throw new NotImplementedException();
        }
    }
}
