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

using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using DiversityPhone.Interface;
using DiversityPhone.Model;

namespace DiversityPhone.Services.BackgroundTasks
{
    

    public class RefreshVocabularyTask : IRefreshVocabularyTask
    {
        private readonly IVocabularyService Vocabulary;
        private readonly IDiversityServiceClient Repository;
        private readonly INotificationService Notification;

        private const int RETRY_COUNT = 3;

        private bool _HasStarted = false;
        private ISubject<string> _Progress = new Subject<string>();
        private UserCredentials _Credentials;
        public IObservable<Unit> _Execute;

        

        public RefreshVocabularyTask(
            IVocabularyService Vocabulary,
            IDiversityServiceClient Repository,
            INotificationService Notification
            )
        {
            this.Vocabulary = Vocabulary;
            this.Repository = Repository;
            this.Notification = Notification;
        }

        public IObservable<Unit> Start(
            UserCredentials login
            )
        {
            lock (this)
            {
                if (!_HasStarted)
                {

                    _Credentials = login;

                    if (_Credentials != null)
                    {
                        Notification.showProgress(_Progress);
                        var execution =
                        Observable.Concat(
                            clearVocabulary(),
                            loadVocabulary(),
                            loadAnalyses(),
                            loadResults(),
                            loadQualifications(),
                            loadProperties())
                            .Finally(() => { if (_Progress != null) _Progress.OnCompleted(); })
                            .Replay();

                        _Execute = execution;

                        execution.Connect();
                    }
                    else
                        throw new ArgumentException("login");
                }

                _HasStarted = true;

                return _Execute;
            }            
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
