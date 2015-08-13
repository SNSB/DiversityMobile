namespace DiversityPhone.Services
{
    using DiversityPhone.DiversityService;
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class RefreshVocabularyTask : IRefreshVocabularyTask
    {
        private readonly ISettingsService Settings;
        private readonly IVocabularyService Vocabulary;
        private readonly IDiversityServiceClient Repository;
        private readonly INotificationService Notification;

        private const int RETRY_COUNT = 
#if DEBUG
            1;
#else
            3;
#endif

        private bool _HasStarted = false;
        private IObservable<string> _Progress;

        public RefreshVocabularyTask(
            ISettingsService Settings,
            IVocabularyService Vocabulary,
            IDiversityServiceClient Repository,
            INotificationService Notification
            )
        {
            this.Settings = Settings;
            this.Vocabulary = Vocabulary;
            this.Repository = Repository;
            this.Notification = Notification;
        }

        public IObservable<Unit> Start()
        {
            lock (this)
            {
                if (!_HasStarted)
                {
                    var status = Settings.CurrentSettings()
                        .Take(1)
                        .Where(x => x != null)
                        .Select(s => s.ToCreds())
                        .SelectMany(creds =>
                            Observable.Concat(
                            clearVocabulary(),
                            loadVocabulary(),
                            loadAnalyses(creds),
                            loadResults(creds),
                            loadQualifications(),
                            loadProperties())
                            )
                        .Replay();

                    Notification.showProgress(status);

                    _Progress = status;

                    status.Connect();
                }

                _HasStarted = true;

                return _Progress
                    .Select(_ => Unit.Default);
            }
        }

        private IObservable<string> clearVocabulary()
        {
            return WithStatus(DiversityResources.RefreshVocabularyTask_State_Cleaning,
                Observable.Defer(() => Observable.Start(Vocabulary.clearVocabulary)));
        }

        private IObservable<string> loadVocabulary()
        {
            return WithStatus(DiversityResources.RefreshVocabularyTask_State_LoadingVocabulary, 
                    Repository.GetStandardVocabulary()
                    .Retry(RETRY_COUNT)
                    .Do(voc => Vocabulary.addTerms(voc))
                );
        }

        private IObservable<string> loadAnalyses(UserCredentials creds)
        {
            return WithStatus(DiversityResources.RefreshVocabularyTask_State_LoadingAnalyses,
                    Repository.GetAnalysesForProject(creds.ProjectID)
                    .Retry(RETRY_COUNT)
                    .Do(analyses => Vocabulary.addAnalyses(analyses))
                );
        }

        private IObservable<string> loadResults(UserCredentials creds)
        {
            return WithStatus(DiversityResources.RefreshVocabularyTask_State_LoadingResults,
                Observable.Concat(
                     Repository.GetAnalysisResultsForProject(creds.ProjectID)
                     .Retry(RETRY_COUNT)
                     .Do(results => Vocabulary.addAnalysisResults(results))
                     .Select(_ => Unit.Default),

                     Repository.GetAnalysisTaxonomicGroupsForProject(creds.ProjectID)
                     .Retry(RETRY_COUNT)
                     .Do(atgs => Vocabulary.addAnalysisTaxonomicGroups(atgs))
                     .Select(_ => Unit.Default)
                     )
                );
        }

        private IObservable<string> loadQualifications()
        {
            return WithStatus(DiversityResources.RefreshVocabularyTask_State_LoadingQualifications,
                    Repository.GetQualifications()
                    .Retry(RETRY_COUNT)
                    .Do(qualifications => Vocabulary.addQualifications(qualifications))
                );
        }

        private IObservable<string> loadProperties()
        {
            return WithStatus(DiversityResources.RefreshVocabularyTask_State_LoadingProperties,
                Repository.GetPropertiesForUser()
                .Retry(RETRY_COUNT)
                .FirstAsync()
                .Do(properties => Vocabulary.addProperties(properties))
                .SelectMany(properties => Observable.Concat(
                    from p in properties
                    select Repository.DownloadPropertyValuesChunked(p)
                            .Retry(RETRY_COUNT)
                            )
                )
                .Do(values => Vocabulary.addPropertyNames(values))
                );
        }

        private IObservable<string> WithStatus<T>(string status, IObservable<T> obs)
        {
            return obs
                .IgnoreElements()
                .Select(_ => string.Empty)
                .StartWith(status);
        }

    }
}