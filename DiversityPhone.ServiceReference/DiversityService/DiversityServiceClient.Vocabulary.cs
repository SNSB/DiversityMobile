using DiversityPhone.DiversityService;
using DiversityPhone.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Client = DiversityPhone.Model;
using Service = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        public IObservable<Client.UserProfile> GetUserInfo(Client.UserCredentials login)
        {
            return DiversityServiceCall((Service.GetUserInfoCompletedEventArgs args) => args.Result, svc => {
                svc.GetUserInfoAsync(login);
            });
        }

        public IObservable<IEnumerable<string>> GetRepositories(Client.UserCredentials login)
        {
            return DiversityServiceCall((Service.GetRepositoriesCompletedEventArgs args) =>
            {
                var result = args.Result ?? Enumerable.Empty<Repository>();
                return from repo in result
                       select repo.DisplayText;
            }, svc =>
            {
                svc.GetRepositoriesAsync(login);
            });
        }

        public IObservable<IList<Client.Project>> GetProjectsForUser(Client.UserCredentials login)
        {
            return DiversityServiceCall((Service.GetProjectsForUserCompletedEventArgs args) => args.Result as IList<Client.Project>, svc => {
                svc.GetProjectsForUserAsync(login);
            });
        }

        public IObservable<IEnumerable<Client.TaxonList>> GetTaxonLists()
        {
            return DiversityServiceCallObservable((IObservable<Service.GetTaxonListsForUserCompletedEventArgs> o) => o
                .Select(args => args.Result ?? Enumerable.Empty<TaxonList>())
                .Select(res => res
                    .Select(svcList => new Client.TaxonList()
                    {
                        ListID = svcList.Id,
                        IsPublicList = svcList.IsPublicList,
                        TableDisplayName = svcList.DisplayText,
                        TableName = svcList.Table,
                        TaxonomicGroup = svcList.TaxonomicGroup
                    }
                    )), svc => WithCredentials(c => svc.GetTaxonListsForUserAsync(c))
                );
        }

        public IObservable<IEnumerable<Client.TaxonName>> DownloadTaxonListChunked(Client.TaxonList list)
        {
            var MAX_RETRIES = 3;

            var serviceList = new TaxonList() { Id = list.ListID ?? 0, DisplayText = list.TableDisplayName, IsPublicList = list.IsPublicList, Table = list.TableName, TaxonomicGroup = list.TaxonomicGroup };
            var svc = new DiversityService.DiversityServiceClient();

            var onCompleted = Observable.FromEventPattern<Service.DownloadTaxonListCompletedEventArgs>(svc, "DownloadTaxonListCompleted", ThreadPool)
                .LogErrors(this)
                .Select(p => p.EventArgs)
                .PipeErrors()
                .Select(args => args.Result)
                // When the first empty chunk is returned, we are done
                .TakeWhile(chunk => chunk != null && chunk.Any());

            var chunkCompleted = onCompleted 
                .Retry(MAX_RETRIES)
                .Select(taxa => 
                    from taxon in taxa
                    select new Client.TaxonName()
                        {
                            GenusOrSupragenic = taxon.GenusOrSupragenic,
                            InfraspecificEpithet = taxon.InfraspecificEpithet,
                            SpeciesEpithet = taxon.SpeciesEpithet,
                            Synonymy = (Client.Synonymy)Enum.Parse(typeof(Client.Synonymy), taxon.Synonymy ?? Client.Synonymy.WorkingName.ToString(), true),
                            TaxonNameCache = taxon.TaxonNameCache,
                            TaxonNameSinAuth = taxon.TaxonNameSinAuth,
                            URI = taxon.URI,
                            AcceptedNameURI = taxon.AcceptedNameURI,
                            AcceptedNameCache = taxon.AcceptedNameCache
                        })
                .Replay();

            chunkCompleted.Connect();

            // The first chunk is 1 not 0!
            var nextChunk = 1;
            onCompleted
                .Select(_ => true)
                .StartWith(false) // Retry Marker
                .Retry(MAX_RETRIES)
                .Select(increment => (increment) ? ++nextChunk : nextChunk)
                .CatchEmpty()
                .Subscribe(chunk => WithCredentials(c => svc.DownloadTaxonListAsync(serviceList, chunk, c)));

            return chunkCompleted;
        }

        public IObservable<IEnumerable<Client.Property>> GetPropertiesForUser()
        {
            return DiversityServiceCallObservable((IObservable<Service.GetPropertiesForUserCompletedEventArgs> o) => o
                .Select(args => args.Result ?? Enumerable.Empty<Property>())
                .Select(list => 
                    from p in list 
                    select new Client.Property()
                    {
                        PropertyID = p.PropertyID,
                        DisplayText = p.DisplayText
                    }), svc => WithCredentials(c => svc.GetPropertiesForUserAsync(c))
                );
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
                        WithCredentials(c => localclient.DownloadPropertyNamesAsync(svcProperty, chunk, c)); // Re-Request last chunk
                        return obs;
                    })
                .TakeWhile(taxonChunk =>
                {
                    if (taxonChunk.Any())
                    {
                        //There might still be more Taxa -> request next chunk
                        WithCredentials(c => localclient.DownloadPropertyNamesAsync(svcProperty, ++chunk, c));
                        return true;
                    }
                    else //Transfer finished
                        return false;
                });
            //Request first chunk
            WithCredentials(c => localclient.DownloadPropertyNamesAsync(svcProperty, chunk, c));
            return res;
        }

        public IObservable<IEnumerable<Client.Term>> GetStandardVocabulary()
        {
            return DiversityServiceCallObservable((IObservable<Service.GetStandardVocabularyCompletedEventArgs> o) => o
                .Select(args => args.Result ?? Enumerable.Empty<Term>())
                .Select(list => 
                    from term in list 
                    select new Client.Term()
                   {
                       Code = term.Code,
                       Description = term.Description,
                       DisplayText = term.DisplayText,
                       ParentCode = term.ParentCode,
                       SourceID = (Client.TermList)term.Source,
                   }), svc => WithCredentials(c => svc.GetStandardVocabularyAsync(c))
                );
        }

        public IObservable<IEnumerable<Client.Analysis>> GetAnalysesForProject(int projectID)
        {
            return DiversityServiceCallObservable((IObservable<Service.GetAnalysesForProjectCompletedEventArgs> o) => o
                .Select(args => args.Result ?? Enumerable.Empty<Analysis>())
                .Select(list => 
                    from an in list
                    select new Client.Analysis()
                    {
                        AnalysisID = an.AnalysisID,
                        Description = an.Description,
                        DisplayText = an.DisplayText,
                        MeasurementUnit = an.MeasurementUnit
                    }), svc => WithCredentials(c => svc.GetAnalysesForProjectAsync(projectID, c))
                );
        }

        public IObservable<IEnumerable<Client.AnalysisResult>> GetAnalysisResultsForProject(int projectID)
        {
            return DiversityServiceCallObservable((IObservable<Service.GetAnalysisResultsForProjectCompletedEventArgs> o) => o
                .Select(args => args.Result ?? Enumerable.Empty<AnalysisResult>())
                .Select(list => 
                    from ar in list
                    select new Client.AnalysisResult()
                    {
                        AnalysisID = ar.AnalysisID,
                        Description = ar.Description,
                        DisplayText = ar.DisplayText,
                        Notes = ar.Notes,
                        Result = ar.Result
                    }), svc => WithCredentials(c => svc.GetAnalysisResultsForProjectAsync(projectID, c))
                );
        }

        public IObservable<IEnumerable<Client.AnalysisTaxonomicGroup>> GetAnalysisTaxonomicGroupsForProject(int projectID)
        {
            return DiversityServiceCallObservable((IObservable<Service.GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs> o) => o
                .Select(args => args.Result ?? Enumerable.Empty<AnalysisTaxonomicGroup>())
                .Select(list => 
                    from atg in list
                    select new Client.AnalysisTaxonomicGroup()
                    {
                        AnalysisID = atg.AnalysisID,
                        TaxonomicGroup = atg.TaxonomicGroup
                    }), svc => WithCredentials(c => svc.GetAnalysisTaxonomicGroupsForProjectAsync(projectID, c))
                );
        }

        public IObservable<IEnumerable<Client.Qualification>> GetQualifications()
        {
            return DiversityServiceCallObservable((IObservable<Service.GetQualificationsCompletedEventArgs> o) => o
                .Select(args => args.Result ?? Enumerable.Empty<Qualification>())
                .Select(list =>
                    from q in list
                    select new Client.Qualification()
                    {
                        Code = q.Code,
                        DisplayText = q.DisplayText
                    }), svc => WithCredentials(c => svc.GetQualificationsAsync(c))
                );
        }
    }
}