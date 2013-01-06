
using System.Collections.Generic;
using Client = DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using DiversityPhone.DiversityService;
using DiversityPhone.MultimediaService;
using System.Linq;
using ReactiveUI;
using System.ComponentModel;
using System.Reactive;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        DiversityService.DiversityServiceClient _svc = new DiversityService.DiversityServiceClient();
        MapService.PhoneMediaServiceClient _maps = new MapService.PhoneMediaServiceClient();
        MultimediaService.MediaService4Client _multimedia = new MultimediaService.MediaService4Client();


        //VOCABULARY
        IObservable<GetTaxonListsForUserCompletedEventArgs> GetTaxonListsForUser;
        IObservable<DownloadTaxonListCompletedEventArgs> DownloadTaxonList;
        IObservable<GetQualificationsCompletedEventArgs> GetQualificationsCompleted;

        // UPLOAD
        IObservable<EventPattern<AsyncCompletedEventArgs>> InsertMMOCompleted;
        IObservable<EventPattern<InsertEventSeriesCompletedEventArgs>> InsertESCompleted;
        IObservable<EventPattern<InsertEventCompletedEventArgs>> InsertEVCompleted;
        IObservable<EventPattern<InsertSpecimenCompletedEventArgs>> InsertSPCompleted;
        IObservable<EventPattern<InsertIdentificationUnitCompletedEventArgs>> InsertIUCompleted;

        //DOWNLOAD

        //MULTIMEDIA
        IObservable<EventPattern<MultimediaService.SubmitCompletedEventArgs>> UploadMultimediaCompleted;


        

        IMessageBus Messenger;
        IKeyMappingService Mapping;
        ObservableAsPropertyHelper<UserCredentials> LatestCreds;

        private UserCredentials GetCreds() { return LatestCreds.Value; }

        public DiversityServiceClient(IMessageBus messenger, IKeyMappingService mapping)
        {
            Messenger = messenger;
            Mapping = mapping;
            LatestCreds = new ObservableAsPropertyHelper<UserCredentials>(messenger.Listen<UserCredentials>(), _ => { });

            GetTaxonListsForUser = Observable.FromEvent<EventHandler<GetTaxonListsForUserCompletedEventArgs>, GetTaxonListsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d);
            DownloadTaxonList = Observable.FromEvent<EventHandler<DownloadTaxonListCompletedEventArgs>, DownloadTaxonListCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.DownloadTaxonListCompleted += d, d => _svc.DownloadTaxonListCompleted -= d);
            GetQualificationsCompleted = Observable.FromEvent<EventHandler<GetQualificationsCompletedEventArgs>, GetQualificationsCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetQualificationsCompleted += d, d => _svc.GetQualificationsCompleted -= d);

            InsertMMOCompleted = Observable.FromEventPattern<AsyncCompletedEventArgs>(h => _svc.InsertMMOCompleted += h, h => _svc.InsertMMOCompleted -= h);
            InsertESCompleted = Observable.FromEventPattern<InsertEventSeriesCompletedEventArgs>(h => _svc.InsertEventSeriesCompleted += h, h => _svc.InsertEventSeriesCompleted -= h);
            InsertEVCompleted = Observable.FromEventPattern<InsertEventCompletedEventArgs>(h => _svc.InsertEventCompleted += h, h => _svc.InsertEventCompleted -= h);
            InsertSPCompleted = Observable.FromEventPattern<InsertSpecimenCompletedEventArgs>(h => _svc.InsertSpecimenCompleted += h, h => _svc.InsertSpecimenCompleted -= h);
            InsertIUCompleted = Observable.FromEventPattern<InsertIdentificationUnitCompletedEventArgs>(h => _svc.InsertIdentificationUnitCompleted += h, h => _svc.InsertIdentificationUnitCompleted -= h);

            UploadMultimediaCompleted = Observable.FromEventPattern<SubmitCompletedEventArgs>(h => _multimedia.SubmitCompleted += h, h => _multimedia.SubmitCompleted -= h);
        }

        private static IObservable<T> ToResultObservable<T>(IObservable<T> source)
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
            var res = ToResultObservable(source);
            _svc.GetUserInfoAsync(login);
            return res;
        }

        public IObservable<IList<Repository>> GetRepositories(DiversityService.UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetRepositoriesCompletedEventArgs>, GetRepositoriesCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetRepositoriesCompleted += d, d => _svc.GetRepositoriesCompleted -= d)
                .Select(args => args.Result as IList<Repository>);
            var res = ToResultObservable(source);
            _svc.GetRepositoriesAsync(login);
            return res;
        }

        public IObservable<IList<Project>> GetProjectsForUser(DiversityService.UserCredentials login)
        {
            var source = Observable.FromEvent<EventHandler<GetProjectsForUserCompletedEventArgs>, GetProjectsForUserCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.GetProjectsForUserCompleted += d, d => _svc.GetProjectsForUserCompleted -= d)
                .Select(args => args.Result as IList<Project>);
            var res = ToResultObservable(source);
            _svc.GetProjectsForUserAsync(login);
            return res;
        }

        public IObservable<IEnumerable<Client.TaxonList>> GetTaxonLists()
        {
            var requestToken = new object();
            var source = 
            GetTaxonListsForUser.Where(args => args.UserState == requestToken).Select(args => args.Result ?? Enumerable.Empty<TaxonList>())
                .Select(res => res
                    .Select(svcList => new Client.TaxonList()
                    {
                        IsPublicList = svcList.IsPublicList,
                        TableDisplayName = svcList.DisplayText,
                        TableName = svcList.Table,
                        TaxonomicGroup =svcList.TaxonomicGroup
                    }
                    ));
            var obs = ToResultObservable(source);
            _svc.GetTaxonListsForUserAsync(GetCreds(), requestToken);
            return obs;
        }

        public IObservable<IEnumerable<Client.TaxonName>> DownloadTaxonListChunked(Client.TaxonList list)
        {
            var serviceList = new TaxonList() { DisplayText = list.TableDisplayName, IsPublicList = list.IsPublicList, Table = list.TableName, TaxonomicGroup = list.TaxonomicGroup };            

            return Observable.Create((IObserver<IEnumerable<Client.TaxonName>> observer) =>
                {
                    int chunk = 1; //First Chunk is 1, not 0!
                    var subscription =
                    DownloadTaxonList
                    .Where(args => Object.ReferenceEquals(args.UserState, list))
                    .Select(args => args.Result ?? Enumerable.Empty<TaxonName>())
                    .Select(taxa => taxa.Select(
                        taxon => new Client.TaxonName()
                        {
                            GenusOrSupragenic = taxon.GenusOrSupragenic,
                            InfraspecificEpithet = taxon.InfraspecificEpithet,
                            SpeciesEpithet = taxon.SpeciesEpithet,
                            Synonymy = (Client.Synonymy)Enum.Parse(typeof(Client.Synonymy), taxon.Synonymy, true),
                            TaxonNameCache = taxon.TaxonNameCache,
                            TaxonNameSinAuth = taxon.TaxonNameSinAuth,
                            URI = taxon.URI,
                            AcceptedNameURI=taxon.AcceptedNameURI,
                            AcceptedNameCache=taxon.AcceptedNameCache
                        }))
                    .TakeWhile(taxonChunk =>
                        {
                            if (taxonChunk.Any())
                            {
                                //There might still be more Taxa -> request next chunk
                                _svc.DownloadTaxonListAsync(serviceList, ++chunk, GetCreds(), list);
                                return true;
                            }
                            else //Transfer finished
                                return false;
                        }).Subscribe(observer);
                    //Request first chunk
                    _svc.DownloadTaxonListAsync(serviceList, chunk, GetCreds(), list);
                    return subscription;
                });
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
            var res = ToResultObservable(source);
            _svc.GetPropertiesForUserAsync(login);
            return res;
        }


        public IObservable<IEnumerable<Client.PropertyName>> DownloadPropertyValuesChunked(Client.Property p)
        {            
            var localclient = new DiversityService.DiversityServiceClient(); //Avoid race conditions from chunked download
            var svcProperty = new Property()
            {
                PropertyID = p.PropertyID,               
                DisplayText = p.DisplayText
            };
            int chunk = 1; //First Chunk is 1, not 0!
            Func<IObservable<IEnumerable<Client.PropertyName>>> factory = () =>
                {
                    var obs = Observable.FromEvent<EventHandler<DownloadPropertyNamesCompletedEventArgs>, DownloadPropertyNamesCompletedEventArgs>((a) => (s, args) => a(args), d => localclient.DownloadPropertyNamesCompleted += d, d => localclient.DownloadPropertyNamesCompleted -= d)
                        .Select(args => args.Result ?? Enumerable.Empty<PropertyValue>())
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
            var res = ToResultObservable(source);
               
            _svc.GetStandardVocabularyAsync(GetCreds());
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
            var res = ToResultObservable(source);            
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
            var res = ToResultObservable(source);
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
            var res = ToResultObservable(source);

            _svc.GetAnalysisTaxonomicGroupsForProjectAsync(projectID, login);
            return res;
        }


        public IObservable<IEnumerable<Client.Qualification>> GetQualifications(UserCredentials credentials)
        {
            var request = new object();
            var res = ToResultObservable(
                GetQualificationsCompleted.Where(args => args.UserState == request)
                .Select(args => args.Result.Select(q => new Client.Qualification()
                    {
                        Code = q.Code,
                        DisplayText = q.DisplayText
                    })));
            _svc.GetQualificationsAsync(credentials, request);
            return res;
        }
    }
}
