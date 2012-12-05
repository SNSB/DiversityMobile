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
using DiversityPhone.DiversityService;
using Funq;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive;

namespace DiversityPhone.Services.BackgroundTasks
{
    public class RefreshVocabularyTask
    {
        private ISubject<string> _Progress;
        private UserCredentials _Credentials;
        private IVocabularyService Vocabulary;
        private IDiversityServiceClient Repository;
        private INotificationService Notification;
        public IObservable<Unit> _Execute;

        public static IObservable<Unit> Start(Container ioc, UserCredentials credentials)
        {
            var task = new RefreshVocabularyTask(ioc, credentials);
            var execution = task._Execute.Replay();
            execution.Connect();

            return execution;
        }

        private RefreshVocabularyTask(Container ioc, UserCredentials credentials)
        {
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Repository = ioc.Resolve<IDiversityServiceClient>();
            Notification = ioc.Resolve<INotificationService>();
            _Credentials = credentials;

            if (_Credentials != null)
            {
                setupProgress();
                _Execute =
                Observable.Concat(
                    clearVocabulary(),
                    loadVocabulary(),
                    loadAnalyses(),
                    loadResults(),                    
                    loadQualifications(),
                    loadProperties())
                    .Finally(() => { if (_Progress != null) _Progress.OnCompleted(); })
                    .ObserveOnDispatcher();
            }
            else
                throw new ArgumentException("credentials");
        }

        private const int RETRY_COUNT = 3;

        void setupProgress()
        {            
            _Progress = new Subject<string>();
            Notification.showProgress(_Progress);
        }

        IObservable<Unit> clearVocabulary()
        {
            return DeferStart(() =>
                {
                    _Progress.OnNext(DiversityResources.RefreshVocabularyTask_State_Cleaning);
                    Vocabulary.clearVocabulary();
                });
        }

        IObservable<Unit> loadVocabulary()
        {
            return DeferStart(() =>
                {
                    _Progress.OnNext(DiversityResources.RefreshVocabularyTask_State_LoadingVocabulary);
                    var voc = Repository.GetStandardVocabulary().Retry(RETRY_COUNT).First();

                    Vocabulary.addTerms(voc);
                });

        }

        IObservable<Unit> loadAnalyses()
        {
            return DeferStart(() =>
                {
                    _Progress.OnNext(DiversityResources.RefreshVocabularyTask_State_LoadingAnalyses);
                    var analyses = Repository.GetAnalysesForProject(_Credentials.ProjectID, _Credentials).Retry(RETRY_COUNT).First();
                    Vocabulary.addAnalyses(analyses);
                });
        }

        IObservable<Unit> loadResults()
        {
            return DeferStart(() =>
                {
                    _Progress.OnNext(DiversityResources.RefreshVocabularyTask_State_LoadingResults);
                    var results = Repository.GetAnalysisResultsForProject(_Credentials.ProjectID, _Credentials).Retry(RETRY_COUNT).First();
                    Vocabulary.addAnalysisResults(results);

                    var atgs = Repository.GetAnalysisTaxonomicGroupsForProject(_Credentials.ProjectID, _Credentials).Retry(RETRY_COUNT).First();
                    Vocabulary.addAnalysisTaxonomicGroups(atgs);
                });
        }

        IObservable<Unit> loadQualifications()
        {
            return DeferStart(() =>
                {
                    _Progress.OnNext(DiversityResources.RefreshVocabularyTask_State_LoadingQualifications);
                    var qualifications = Repository.GetQualifications(_Credentials).Retry(RETRY_COUNT).First();
                    Vocabulary.addQualifications(qualifications);
                });
        }

        IObservable<Unit> loadProperties()
        {
            return DeferStart(() =>
                {
                    _Progress.OnNext(DiversityResources.RefreshVocabularyTask_State_LoadingProperties);
                    var properties = Repository.GetPropertiesForUser(_Credentials).Retry(RETRY_COUNT).First();
                    Vocabulary.addProperties(properties);

                    foreach (var p in properties)
                    {
                        Repository.DownloadPropertyValuesChunked(p)
                            .ForEach(chunk => Vocabulary.addPropertyNames(chunk));
                    }
                });
        }

        private static IObservable<Unit> DeferStart(Action a)
        {
            return Observable.Defer(() => Observable.Start(a));
        }
    }
}
