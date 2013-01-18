
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
using System.Reactive.Disposables;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        DiversityService.DiversityServiceClient _svc = new DiversityService.DiversityServiceClient();
        MapService.PhoneMediaServiceClient _maps = new MapService.PhoneMediaServiceClient();
        MultimediaService.MediaService4Client _multimedia = new MultimediaService.MediaService4Client();

        //MISC
        IObservable<EventPattern<GetUserInfoCompletedEventArgs>> GetUserInfoCompleted;
        IObservable<EventPattern<GetRepositoriesCompletedEventArgs>> GetRepositoriesCompleted;
        IObservable<EventPattern<GetPropertiesForUserCompletedEventArgs>> GetPropertiesForUserCompleted;
        IObservable<EventPattern<GetProjectsForUserCompletedEventArgs>> GetProjectsForUserCompleted;


        //VOCABULARY
        IObservable<EventPattern<GetStandardVocabularyCompletedEventArgs>> GetStandardVocabularyCompleted;
        IObservable<EventPattern<GetTaxonListsForUserCompletedEventArgs>> GetTaxonListsForUser;
        IObservable<EventPattern<DownloadTaxonListCompletedEventArgs>> DownloadTaxonList;
        IObservable<EventPattern<GetQualificationsCompletedEventArgs>> GetQualificationsCompleted;
        IObservable<EventPattern<GetAnalysesForProjectCompletedEventArgs>> GetAnalysesForProjectCompleted;
        IObservable<EventPattern<GetAnalysisResultsForProjectCompletedEventArgs>> GetAnalysisResultsForProjectCompleted;
        IObservable<EventPattern<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>> GetAnalysisTaxonomicGroupsForProjectCompleted;

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

            GetUserInfoCompleted = Observable.FromEventPattern<GetUserInfoCompletedEventArgs>(h => _svc.GetUserInfoCompleted += h, h => _svc.GetUserInfoCompleted -= h);
            GetRepositoriesCompleted = Observable.FromEventPattern<GetRepositoriesCompletedEventArgs>(h => _svc.GetRepositoriesCompleted += h, h => _svc.GetRepositoriesCompleted -= h);
            GetPropertiesForUserCompleted = Observable.FromEventPattern<GetPropertiesForUserCompletedEventArgs>(h => _svc.GetPropertiesForUserCompleted += h, h => _svc.GetPropertiesForUserCompleted -= h);
            GetProjectsForUserCompleted = Observable.FromEventPattern<GetProjectsForUserCompletedEventArgs>(h => _svc.GetProjectsForUserCompleted += h, h => _svc.GetProjectsForUserCompleted -= h);
            GetStandardVocabularyCompleted = Observable.FromEventPattern<GetStandardVocabularyCompletedEventArgs>(d => _svc.GetStandardVocabularyCompleted += d, d => _svc.GetStandardVocabularyCompleted -= d);
            GetAnalysesForProjectCompleted = Observable.FromEventPattern<GetAnalysesForProjectCompletedEventArgs>(d => _svc.GetAnalysesForProjectCompleted += d, d => _svc.GetAnalysesForProjectCompleted -= d);
            GetAnalysisResultsForProjectCompleted = Observable.FromEventPattern<GetAnalysisResultsForProjectCompletedEventArgs>( d => _svc.GetAnalysisResultsForProjectCompleted += d, d => _svc.GetAnalysisResultsForProjectCompleted -= d);
            GetAnalysisTaxonomicGroupsForProjectCompleted = Observable.FromEventPattern<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>(d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted += d, d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted -= d);

            GetTaxonListsForUser = Observable.FromEventPattern<GetTaxonListsForUserCompletedEventArgs>( d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d);
            DownloadTaxonList = Observable.FromEventPattern<DownloadTaxonListCompletedEventArgs>( d => _svc.DownloadTaxonListCompleted += d, d => _svc.DownloadTaxonListCompleted -= d);
            GetQualificationsCompleted = Observable.FromEventPattern<GetQualificationsCompletedEventArgs>( d => _svc.GetQualificationsCompleted += d, d => _svc.GetQualificationsCompleted -= d);

            InsertMMOCompleted = Observable.FromEventPattern<AsyncCompletedEventArgs>(h => _svc.InsertMMOCompleted += h, h => _svc.InsertMMOCompleted -= h);
            InsertESCompleted = Observable.FromEventPattern<InsertEventSeriesCompletedEventArgs>(h => _svc.InsertEventSeriesCompleted += h, h => _svc.InsertEventSeriesCompleted -= h);
            InsertEVCompleted = Observable.FromEventPattern<InsertEventCompletedEventArgs>(h => _svc.InsertEventCompleted += h, h => _svc.InsertEventCompleted -= h);
            InsertSPCompleted = Observable.FromEventPattern<InsertSpecimenCompletedEventArgs>(h => _svc.InsertSpecimenCompleted += h, h => _svc.InsertSpecimenCompleted -= h);
            InsertIUCompleted = Observable.FromEventPattern<InsertIdentificationUnitCompletedEventArgs>(h => _svc.InsertIdentificationUnitCompleted += h, h => _svc.InsertIdentificationUnitCompleted -= h);

            UploadMultimediaCompleted = Observable.FromEventPattern<SubmitCompletedEventArgs>(h => _multimedia.SubmitCompleted += h, h => _multimedia.SubmitCompleted -= h);

            InsertESCompleted
                .OnErrorResumeNext(InsertESCompleted)
                .Where(p => !(p.EventArgs.Error != null || p.EventArgs.Cancelled))
                .Subscribe(p =>
                    {
                        Client.EventSeries es = p.EventArgs.UserState as Client.EventSeries;
                        if (es != null)
                            Mapping.AddMapping(Client.DBObjectType.EventSeries, es.SeriesID.Value, p.EventArgs.Result);
                    });

            InsertEVCompleted
                .OnErrorResumeNext(InsertEVCompleted)
                .Where(p => !(p.EventArgs.Error != null || p.EventArgs.Cancelled))
                .Subscribe(p =>
                {
                    Client.Event ev = p.EventArgs.UserState as Client.Event;
                    if (ev != null)
                        Mapping.AddMapping(Client.DBObjectType.Event, ev.EventID, p.EventArgs.Result);
                });

            InsertSPCompleted
                .OnErrorResumeNext(InsertSPCompleted)
                .Where(p => !(p.EventArgs.Error != null || p.EventArgs.Cancelled))
                .Subscribe(p =>
                {
                    var sp = p.EventArgs.UserState as Client.Specimen;
                    if (sp != null)
                        Mapping.AddMapping(Client.DBObjectType.Specimen, sp.SpecimenID, p.EventArgs.Result);
                });

            InsertIUCompleted
                .OnErrorResumeNext(InsertIUCompleted)
                .Where(p => !(p.EventArgs.Error != null || p.EventArgs.Cancelled))
                .Subscribe(p =>
                {
                    var iu = p.EventArgs.UserState as Client.IdentificationUnit;
                    if (iu != null)
                        Mapping.AddMapping(Client.DBObjectType.IdentificationUnit, iu.UnitID, p.EventArgs.Result);
                });
        }

        private static IObservable<TEventArgs> FilterByUserStatePipeErrorsAndReplayOne<TEventArgs>(IObservable<EventPattern<TEventArgs>> serviceStream, object userState) where TEventArgs : AsyncCompletedEventArgs
        {
            var res = PipeErrors(
                serviceStream.Where(p => p.EventArgs.UserState == userState)
                ).Take(1).Replay(1);
            res.Connect();
            return res;
        }

        private static IObservable<TEventArgs> PipeErrors<TEventArgs>(IObservable<EventPattern<TEventArgs>> serviceStream) where TEventArgs : AsyncCompletedEventArgs
        {
            return Observable.Create<TEventArgs>(obs =>
                {
                    if(obs == null)
                        throw new ArgumentNullException("obs");

                    IDisposable subscription = null;
                    try
                    {
                        subscription = serviceStream.Subscribe(
                            p =>
                            {
                                if (p.EventArgs.Error != null)
                                    obs.OnError(p.EventArgs.Error);
                                else if (p.EventArgs.Cancelled)
                                    obs.OnCompleted();
                                else
                                    obs.OnNext(p.EventArgs);
                            },
                            obs.OnError,
                            obs.OnCompleted);                        
                    }
                    catch(Exception ex)
                    {
                        obs.OnError(ex);
                        if (subscription != null)
                            subscription.Dispose();
                    }
                    return subscription;
                });
        }

        

        public IObservable<UserProfile> GetUserInfo(UserCredentials login)
        {
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetUserInfoCompleted, login)
                .Select(args => args.Result);
            _svc.GetUserInfoAsync(login, login);
            return source;
        }

        public IObservable<IList<Repository>> GetRepositories(DiversityService.UserCredentials login)
        {
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetRepositoriesCompleted, login)
                .Select(args => args.Result as IList<Repository>);            
            _svc.GetRepositoriesAsync(login, login);
            return source;
        }

        public IObservable<IList<Project>> GetProjectsForUser(DiversityService.UserCredentials login)
        {
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetProjectsForUserCompleted, login)
                .Select(args => args.Result as IList<Project>);            
            _svc.GetProjectsForUserAsync(login,login);
            return source;
        }

        public IObservable<IEnumerable<Client.TaxonList>> GetTaxonLists()
        {
            var requestToken = new object();
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetTaxonListsForUser, requestToken)
            .Select(args => args.Result ?? Enumerable.Empty<TaxonList>())
                .Select(res => res
                    .Select(svcList => new Client.TaxonList()
                    {
                        IsPublicList = svcList.IsPublicList,
                        TableDisplayName = svcList.DisplayText,
                        TableName = svcList.Table,
                        TaxonomicGroup =svcList.TaxonomicGroup
                    }
                    ));            
            _svc.GetTaxonListsForUserAsync(GetCreds(), requestToken);
            return source;
        }

        public IObservable<IEnumerable<Client.TaxonName>> DownloadTaxonListChunked(Client.TaxonList list)
        {
            var serviceList = new TaxonList() { DisplayText = list.TableDisplayName, IsPublicList = list.IsPublicList, Table = list.TableName, TaxonomicGroup = list.TaxonomicGroup };            

            return Observable.Create((IObserver<IEnumerable<Client.TaxonName>> observer) =>
                {
                    int chunk = 1; //First Chunk is 1, not 0!
                    var subscription = FilterByUserStatePipeErrorsAndReplayOne(DownloadTaxonList, list)                    
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
            var source = FilterByUserStatePipeErrorsAndReplayOne( GetPropertiesForUserCompleted, login)
                .Select(args => args.Result
                    .Select(p => new Client.Property()
                    { 
                        PropertyID = p.PropertyID,                        
                        DisplayText = p.DisplayText
                    }));            
            _svc.GetPropertiesForUserAsync(login,login);
            return source;
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
            object requestToken = new object();
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetStandardVocabularyCompleted, requestToken)
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

            _svc.GetStandardVocabularyAsync(GetCreds(), requestToken);
            return source;
        }

        public IObservable<IEnumerable<Client.Analysis>> GetAnalysesForProject(int projectID, UserCredentials login)
        {
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetAnalysesForProjectCompleted, login)
               .Select(args => args.Result)
               .Select(analyses => analyses
                   .Select(an => new Client.Analysis()
                   {
                       AnalysisID = an.AnalysisID,
                       Description = an.Description,
                       DisplayText = an.DisplayText,
                       MeasurementUnit = an.MeasurementUnit
                   }));                       
            _svc.GetAnalysesForProjectAsync(projectID, login, login);            
            return source;
        }

        public IObservable<IEnumerable<Client.AnalysisResult>> GetAnalysisResultsForProject(int projectID, UserCredentials login)
        {
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetAnalysisResultsForProjectCompleted, login)
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
            _svc.GetAnalysisResultsForProjectAsync(projectID, login, login);
            return source;
        }

        public IObservable<IEnumerable<Client.AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(int projectID, UserCredentials login)
        {
            var source = FilterByUserStatePipeErrorsAndReplayOne(GetAnalysisTaxonomicGroupsForProjectCompleted, login)
               .Select(args => args.Result)
               .Select(atgs => atgs
                   .Select(atg => new Client.AnalysisTaxonomicGroup()
                   {
                       AnalysisID = atg.AnalysisID,
                       TaxonomicGroup = atg.TaxonomicGroup
                   }));
            

            _svc.GetAnalysisTaxonomicGroupsForProjectAsync(projectID, login, login);
            return source;
        }


        public IObservable<IEnumerable<Client.Qualification>> GetQualifications(UserCredentials credentials)
        {
            var request = new object();
            var res = FilterByUserStatePipeErrorsAndReplayOne(GetQualificationsCompleted, request)
                .Select(args => args.Result.Select(q => new Client.Qualification()
                    {
                        Code = q.Code,
                        DisplayText = q.DisplayText
                    }));
            _svc.GetQualificationsAsync(credentials, request);
            return res;
        }
    }
}
