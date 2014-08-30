using DiversityPhone.DiversityService;
using DiversityPhone.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Client = DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        public IObservable<Client.UserProfile> GetUserInfo(Client.UserCredentials login)
        {
            var source = GetUserInfoCompleted.MakeObservableServiceResultSingle(login)
                .Select(args => args.Result);
            _svc.GetUserInfoAsync(login, login);
            return source;
        }

        public IObservable<IEnumerable<string>> GetRepositories(Client.UserCredentials login)
        {
            var source = GetRepositoriesCompleted.MakeObservableServiceResultSingle(login)
                .Select(args => from repo in args.Result
                                select repo.DisplayText);
            _svc.GetRepositoriesAsync(login, login);
            return source;
        }

        public IObservable<IList<Client.Project>> GetProjectsForUser(Client.UserCredentials login)
        {
            var source = GetProjectsForUserCompleted.MakeObservableServiceResultSingle(login)
                .Select(args => args.Result as IList<Client.Project>);
            _svc.GetProjectsForUserAsync(login, login);
            return source;
        }

        public IObservable<IEnumerable<Client.TaxonList>> GetTaxonLists()
        {
            var requestToken = new object();
            var source = GetTaxonListsForUser.MakeObservableServiceResultSingle(requestToken)
            .Select(args => args.Result ?? Enumerable.Empty<TaxonList>())
                .Select(res => res
                    .Select(svcList => new Client.TaxonList()
                    {
                        IsPublicList = svcList.IsPublicList,
                        TableDisplayName = svcList.DisplayText,
                        TableName = svcList.Table,
                        TaxonomicGroup = svcList.TaxonomicGroup
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
                    var subscription = DownloadTaxonList.MakeObservableServiceResult(list)
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
                            AcceptedNameURI = taxon.AcceptedNameURI,
                            AcceptedNameCache = taxon.AcceptedNameCache
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

        public IObservable<IEnumerable<Client.Property>> GetPropertiesForUser(Client.UserCredentials login)
        {
            var source = GetPropertiesForUserCompleted.MakeObservableServiceResultSingle(login)
                .Select(args => args.Result
                    .Select(p => new Client.Property()
                    {
                        PropertyID = p.PropertyID,
                        DisplayText = p.DisplayText
                    }));
            _svc.GetPropertiesForUserAsync(login, login);
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
                                PropertyUri = property.PropertyUri,
                                PropertyID = property.PropertyID,
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
            var source = GetStandardVocabularyCompleted.MakeObservableServiceResultSingle(requestToken)
               .Select(args => args.Result)
               .Select(terms => terms
                   .Select(term => new Client.Term()
                   {
                       Code = term.Code,
                       Description = term.Description,
                       DisplayText = term.DisplayText,
                       ParentCode = term.ParentCode,
                       SourceID = (Client.TermList)term.Source,
                   }));

            _svc.GetStandardVocabularyAsync(GetCreds(), requestToken);
            return source;
        }

        public IObservable<IEnumerable<Client.Analysis>> GetAnalysesForProject(int projectID, Client.UserCredentials login)
        {
            var source = GetAnalysesForProjectCompleted.MakeObservableServiceResultSingle(login)
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

        public IObservable<IEnumerable<Client.AnalysisResult>> GetAnalysisResultsForProject(int projectID, Client.UserCredentials login)
        {
            var source = GetAnalysisResultsForProjectCompleted.MakeObservableServiceResultSingle(login)
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

        public IObservable<IEnumerable<Client.AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(int projectID, Client.UserCredentials login)
        {
            var source = GetAnalysisTaxonomicGroupsForProjectCompleted.MakeObservableServiceResultSingle(login)
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

        public IObservable<IEnumerable<Client.Qualification>> GetQualifications(Client.UserCredentials credentials)
        {
            var request = new object();
            var res = GetQualificationsCompleted.MakeObservableServiceResultSingle(request)
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