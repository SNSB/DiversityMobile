using DiversityPhone.DiversityService;
using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.PhoneMediaService;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using Client = DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient, IEnableLogger
    {
        private DiversityService.DiversityServiceClient _svc = new DiversityService.DiversityServiceClient();
        private PhoneMediaService.PhoneMediaServiceClient _multimedia = new PhoneMediaService.PhoneMediaServiceClient();

        //MISC
        private IObservable<EventPattern<GetUserInfoCompletedEventArgs>> GetUserInfoCompleted;

        private IObservable<EventPattern<GetRepositoriesCompletedEventArgs>> GetRepositoriesCompleted;
        private IObservable<EventPattern<GetPropertiesForUserCompletedEventArgs>> GetPropertiesForUserCompleted;
        private IObservable<EventPattern<GetProjectsForUserCompletedEventArgs>> GetProjectsForUserCompleted;

        //VOCABULARY
        private IObservable<EventPattern<GetStandardVocabularyCompletedEventArgs>> GetStandardVocabularyCompleted;

        private IObservable<EventPattern<GetTaxonListsForUserCompletedEventArgs>> GetTaxonListsForUser;
        private IObservable<EventPattern<DownloadTaxonListCompletedEventArgs>> DownloadTaxonList;
        private IObservable<EventPattern<GetQualificationsCompletedEventArgs>> GetQualificationsCompleted;
        private IObservable<EventPattern<GetAnalysesForProjectCompletedEventArgs>> GetAnalysesForProjectCompleted;
        private IObservable<EventPattern<GetAnalysisResultsForProjectCompletedEventArgs>> GetAnalysisResultsForProjectCompleted;
        private IObservable<EventPattern<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>> GetAnalysisTaxonomicGroupsForProjectCompleted;

        // UPLOAD
        private IObservable<EventPattern<AsyncCompletedEventArgs>> InsertMMOCompleted;

        private IObservable<EventPattern<InsertEventSeriesCompletedEventArgs>> InsertESCompleted;
        private IObservable<EventPattern<InsertEventCompletedEventArgs>> InsertEVCompleted;
        private IObservable<EventPattern<InsertSpecimenCompletedEventArgs>> InsertSPCompleted;
        private IObservable<EventPattern<InsertIdentificationUnitCompletedEventArgs>> InsertIUCompleted;

        //DOWNLOAD
        private IObservable<EventPattern<EventSeriesByQueryCompletedEventArgs>> EventSeriesByQueryCompleted;

        private IObservable<EventPattern<EventSeriesByIDCompletedEventArgs>> EventSeriesByIDCompleted;

        private IObservable<EventPattern<LocalizationsForSeriesCompletedEventArgs>> LocalizationsForSeriesCompleted;
        private IObservable<EventPattern<EventsByLocalityCompletedEventArgs>> EventsByLocalityCompleted;
        private IObservable<EventPattern<PropertiesForEventCompletedEventArgs>> PropertiesForEventCompleted;
        private IObservable<EventPattern<SpecimenForEventCompletedEventArgs>> SpecimenForEventCompleted;
        private IObservable<EventPattern<UnitsForSpecimenCompletedEventArgs>> UnitsForSpecimenCompleted;
        private IObservable<EventPattern<SubUnitsForIUCompletedEventArgs>> SubUnitsForIUCompleted;
        private IObservable<EventPattern<AnalysesForIUCompletedEventArgs>> AnalysesForIUCompleted;

        //MULTIMEDIA
        private IObservable<EventPattern<PhoneMediaService.SubmitCompletedEventArgs>> UploadMultimediaCompleted;

        private IObservable<EventPattern<PhoneMediaService.BeginTransactionCompletedEventArgs>> BeginTransactionCompleted;
        private IObservable<EventPattern<PhoneMediaService.EncodeFileCompletedEventArgs>> EncodeFileCompleted;
        private IObservable<EventPattern<PhoneMediaService.CommitCompletedEventArgs>> CommitCompleted;
        private IObservable<EventPattern<AsyncCompletedEventArgs>> RollbackCompleted;

        private readonly IKeyMappingService Mapping;
        private readonly ICredentialsService Credentials;
        private readonly IScheduler ThreadPool;

        public DiversityServiceClient(ICredentialsService Credentials, IKeyMappingService Mapping, [ThreadPool] IScheduler ThreadPool)
        {
            this.Mapping = Mapping;
            this.Credentials = Credentials;
            this.ThreadPool = ThreadPool;

            GetUserInfoCompleted = Observable.FromEventPattern<GetUserInfoCompletedEventArgs>(h => _svc.GetUserInfoCompleted += h, h => _svc.GetUserInfoCompleted -= h, ThreadPool);
            LogErrors<GetUserInfoCompletedEventArgs>(GetUserInfoCompleted);
            GetRepositoriesCompleted = Observable.FromEventPattern<GetRepositoriesCompletedEventArgs>(h => _svc.GetRepositoriesCompleted += h, h => _svc.GetRepositoriesCompleted -= h, ThreadPool);
            LogErrors<GetRepositoriesCompletedEventArgs>(GetRepositoriesCompleted);
            GetPropertiesForUserCompleted = Observable.FromEventPattern<GetPropertiesForUserCompletedEventArgs>(h => _svc.GetPropertiesForUserCompleted += h, h => _svc.GetPropertiesForUserCompleted -= h, ThreadPool);
            LogErrors<GetPropertiesForUserCompletedEventArgs>(GetPropertiesForUserCompleted);
            GetProjectsForUserCompleted = Observable.FromEventPattern<GetProjectsForUserCompletedEventArgs>(h => _svc.GetProjectsForUserCompleted += h, h => _svc.GetProjectsForUserCompleted -= h, ThreadPool);
            LogErrors<GetProjectsForUserCompletedEventArgs>(GetProjectsForUserCompleted);
            GetStandardVocabularyCompleted = Observable.FromEventPattern<GetStandardVocabularyCompletedEventArgs>(d => _svc.GetStandardVocabularyCompleted += d, d => _svc.GetStandardVocabularyCompleted -= d, ThreadPool);
            LogErrors<GetStandardVocabularyCompletedEventArgs>(GetStandardVocabularyCompleted);
            GetAnalysesForProjectCompleted = Observable.FromEventPattern<GetAnalysesForProjectCompletedEventArgs>(d => _svc.GetAnalysesForProjectCompleted += d, d => _svc.GetAnalysesForProjectCompleted -= d, ThreadPool);
            LogErrors<GetAnalysesForProjectCompletedEventArgs>(GetAnalysesForProjectCompleted);
            GetAnalysisResultsForProjectCompleted = Observable.FromEventPattern<GetAnalysisResultsForProjectCompletedEventArgs>(d => _svc.GetAnalysisResultsForProjectCompleted += d, d => _svc.GetAnalysisResultsForProjectCompleted -= d, ThreadPool);
            LogErrors<GetAnalysisResultsForProjectCompletedEventArgs>(GetAnalysisResultsForProjectCompleted);
            GetAnalysisTaxonomicGroupsForProjectCompleted = Observable.FromEventPattern<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>(d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted += d, d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted -= d, ThreadPool);
            LogErrors<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>(GetAnalysisTaxonomicGroupsForProjectCompleted);

            GetTaxonListsForUser = Observable.FromEventPattern<GetTaxonListsForUserCompletedEventArgs>(d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d, ThreadPool);
            LogErrors<GetTaxonListsForUserCompletedEventArgs>(GetTaxonListsForUser);
            DownloadTaxonList = Observable.FromEventPattern<DownloadTaxonListCompletedEventArgs>(d => _svc.DownloadTaxonListCompleted += d, d => _svc.DownloadTaxonListCompleted -= d, ThreadPool);
            LogErrors<DownloadTaxonListCompletedEventArgs>(DownloadTaxonList);
            GetQualificationsCompleted = Observable.FromEventPattern<GetQualificationsCompletedEventArgs>(d => _svc.GetQualificationsCompleted += d, d => _svc.GetQualificationsCompleted -= d, ThreadPool);
            LogErrors<GetQualificationsCompletedEventArgs>(GetQualificationsCompleted);

            InsertMMOCompleted = Observable.FromEventPattern<AsyncCompletedEventArgs>(h => _svc.InsertMMOCompleted += h, h => _svc.InsertMMOCompleted -= h, ThreadPool);
            LogErrors<AsyncCompletedEventArgs>(InsertMMOCompleted);
            InsertESCompleted = Observable.FromEventPattern<InsertEventSeriesCompletedEventArgs>(h => _svc.InsertEventSeriesCompleted += h, h => _svc.InsertEventSeriesCompleted -= h, ThreadPool);
            LogErrors<InsertEventSeriesCompletedEventArgs>(InsertESCompleted);
            InsertEVCompleted = Observable.FromEventPattern<InsertEventCompletedEventArgs>(h => _svc.InsertEventCompleted += h, h => _svc.InsertEventCompleted -= h, ThreadPool);
            LogErrors<InsertEventCompletedEventArgs>(InsertEVCompleted);
            InsertSPCompleted = Observable.FromEventPattern<InsertSpecimenCompletedEventArgs>(h => _svc.InsertSpecimenCompleted += h, h => _svc.InsertSpecimenCompleted -= h, ThreadPool);
            LogErrors<InsertSpecimenCompletedEventArgs>(InsertSPCompleted);
            InsertIUCompleted = Observable.FromEventPattern<InsertIdentificationUnitCompletedEventArgs>(h => _svc.InsertIdentificationUnitCompleted += h, h => _svc.InsertIdentificationUnitCompleted -= h, ThreadPool);
            LogErrors<InsertIdentificationUnitCompletedEventArgs>(InsertIUCompleted);

            UploadMultimediaCompleted = Observable.FromEventPattern<SubmitCompletedEventArgs>(h => _multimedia.SubmitCompleted += h, h => _multimedia.SubmitCompleted -= h, ThreadPool);
            LogErrors<SubmitCompletedEventArgs>(UploadMultimediaCompleted);
            BeginTransactionCompleted = Observable.FromEventPattern<BeginTransactionCompletedEventArgs>(h => _multimedia.BeginTransactionCompleted += h, h => _multimedia.BeginTransactionCompleted -= h, ThreadPool);
            LogErrors<BeginTransactionCompletedEventArgs>(BeginTransactionCompleted);
            EncodeFileCompleted = Observable.FromEventPattern<EncodeFileCompletedEventArgs>(h => _multimedia.EncodeFileCompleted += h, h => _multimedia.EncodeFileCompleted -= h, ThreadPool);
            LogErrors<EncodeFileCompletedEventArgs>(EncodeFileCompleted);
            CommitCompleted = Observable.FromEventPattern<CommitCompletedEventArgs>(h => _multimedia.CommitCompleted += h, h => _multimedia.CommitCompleted -= h, ThreadPool);
            LogErrors<CommitCompletedEventArgs>(CommitCompleted);
            RollbackCompleted = Observable.FromEventPattern<AsyncCompletedEventArgs>(h => _multimedia.RollbackCompleted += h, h => _multimedia.RollbackCompleted -= h, ThreadPool);

            EventSeriesByQueryCompleted = Observable.FromEventPattern<EventSeriesByQueryCompletedEventArgs>(h => _svc.EventSeriesByQueryCompleted += h, h => _svc.EventSeriesByQueryCompleted -= h, ThreadPool);
            LogErrors<EventSeriesByQueryCompletedEventArgs>(EventSeriesByQueryCompleted);
            EventSeriesByIDCompleted = Observable.FromEventPattern<EventSeriesByIDCompletedEventArgs>(h => _svc.EventSeriesByIDCompleted += h, h => _svc.EventSeriesByIDCompleted -= h, ThreadPool);
            LogErrors<EventSeriesByIDCompletedEventArgs>(EventSeriesByIDCompleted);
            LocalizationsForSeriesCompleted = Observable.FromEventPattern<LocalizationsForSeriesCompletedEventArgs>(h => _svc.LocalizationsForSeriesCompleted += h, h => _svc.LocalizationsForSeriesCompleted -= h, ThreadPool);
            LogErrors<LocalizationsForSeriesCompletedEventArgs>(LocalizationsForSeriesCompleted);
            EventsByLocalityCompleted = Observable.FromEventPattern<EventsByLocalityCompletedEventArgs>(h => _svc.EventsByLocalityCompleted += h, h => _svc.EventsByLocalityCompleted -= h, ThreadPool);
            LogErrors<EventsByLocalityCompletedEventArgs>(EventsByLocalityCompleted);
            PropertiesForEventCompleted = Observable.FromEventPattern<PropertiesForEventCompletedEventArgs>(h => _svc.PropertiesForEventCompleted += h, h => _svc.PropertiesForEventCompleted -= h, ThreadPool);
            LogErrors<PropertiesForEventCompletedEventArgs>(PropertiesForEventCompleted);
            SpecimenForEventCompleted = Observable.FromEventPattern<SpecimenForEventCompletedEventArgs>(h => _svc.SpecimenForEventCompleted += h, h => _svc.SpecimenForEventCompleted -= h, ThreadPool);
            LogErrors<SpecimenForEventCompletedEventArgs>(SpecimenForEventCompleted);
            UnitsForSpecimenCompleted = Observable.FromEventPattern<UnitsForSpecimenCompletedEventArgs>(h => _svc.UnitsForSpecimenCompleted += h, h => _svc.UnitsForSpecimenCompleted -= h, ThreadPool);
            LogErrors<UnitsForSpecimenCompletedEventArgs>(UnitsForSpecimenCompleted);
            SubUnitsForIUCompleted = Observable.FromEventPattern<SubUnitsForIUCompletedEventArgs>(h => _svc.SubUnitsForIUCompleted += h, h => _svc.SubUnitsForIUCompleted -= h, ThreadPool);
            LogErrors<SubUnitsForIUCompletedEventArgs>(SubUnitsForIUCompleted);
            AnalysesForIUCompleted = Observable.FromEventPattern<AnalysesForIUCompletedEventArgs>(h => _svc.AnalysesForIUCompleted += h, h => _svc.AnalysesForIUCompleted -= h, ThreadPool);
            LogErrors<AnalysesForIUCompletedEventArgs>(AnalysesForIUCompleted);
        }

        private void WithCredentials(Action<UserCredentials> action)
        {
            Credentials.CurrentCredentials()
                .Where(c => c != null)
                .FirstAsync()
                .Subscribe(action);
        }

        private void LogErrors<T>(IObservable<EventPattern<T>> EventStream) where T : AsyncCompletedEventArgs
        {
            var logger = this.Log();

            EventStream
                .Subscribe(args =>
                {
                    var error = args.EventArgs.Error;

                    if (error != null)
                    {
                        logger.ErrorException("DiversityService Call Failed", error);
                    }
                });
        }
    }
}