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

namespace DiversityPhone.Services.BackgroundTasks
{
    public class RefreshVocabularyTask : BackgroundTask
    {

        private IVocabularyService Vocabulary;
        private IDiversityServiceClient Repository;

        public RefreshVocabularyTask (Container ioc)
	    {
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Repository = ioc.Resolve<IDiversityServiceClient>();
	    }

        private const string KEY_LOGINNAME = "L";
        private const string KEY_PASSWORD = "P";
        private const string KEY_PROJECT = "J";
        private const string KEY_REPOSITORY = "R";

        private const string KEY_STATE = "S";
        private const string STATE_INITIAL = "0";
        private const string STATE_CLEANED = "1";
        private const string STATE_VOCABULARY_DONE = "2";
        private const string STATE_ANALYSES_DONE = "3";
        private const string STATE_RESULTS_DONE = "4";
        private const string STATE_ALL_DONE = "5";

        private bool stepFinished;

        private string CurrentStep
        {
            get
            {
                string res;
                if (State.TryGetValue(KEY_STATE, out res))
                    return res;
                else
                    return STATE_INITIAL;
            }
            set
            {
                if (!Cancelled)
                    State[KEY_STATE] = value;
            }
        }

        public override bool CanResume
        {
            get { return false; }
        }

        protected override void saveArgumentToState(object arg)
        {
            var user = arg as UserCredentials;
            if (user != null)
            {
                State[KEY_LOGINNAME] = user.LoginName;
                State[KEY_PASSWORD] = user.Password;
                State[KEY_PROJECT] = user.ProjectID.ToString();
                State[KEY_REPOSITORY] = user.Repository;
            }
        }

        protected override object getArgumentFromState()
        {
            return new UserCredentials()
            {
                LoginName = State[KEY_LOGINNAME],
                Password = State[KEY_PASSWORD],
                Repository = State[KEY_REPOSITORY],
                ProjectID = int.Parse(State[KEY_PROJECT])
            };
        }

        protected override void Run(object arg)
        {
            var credentials = arg as UserCredentials;
            if (credentials != null)
            {

                while (CurrentStep != STATE_ALL_DONE)
                {
                    switch (CurrentStep)
                    {
                        case STATE_INITIAL:
                            reportProgress(DiversityResources.RefreshVocabularyTask_State_Cleaning);
                            Vocabulary.clearVocabulary();
                            CurrentStep = STATE_CLEANED;
                            break;
                        case STATE_CLEANED:
                            reportProgress(DiversityResources.RefreshVocabularyTask_State_LoadingVocabulary);
                            var voc = retryOnException(() => Repository.GetStandardVocabulary().First());
                            
                            Vocabulary.addTerms(voc);
                            CurrentStep = STATE_VOCABULARY_DONE;
                            break;
                        case STATE_VOCABULARY_DONE:
                            reportProgress(DiversityResources.RefreshVocabularyTask_State_LoadingAnalyses);
                            var analyses = retryOnException(() => Repository.GetAnalysesForProject(credentials.ProjectID, credentials).First());
                            Vocabulary.addAnalyses(analyses);
                            CurrentStep = STATE_ANALYSES_DONE;
                            break;
                        case STATE_ANALYSES_DONE:
                            reportProgress(DiversityResources.RefreshVocabularyTask_State_LoadingResults);
                            var results = retryOnException(() => Repository.GetAnalysisResultsForProject(credentials.ProjectID, credentials).First());
                            Vocabulary.addAnalysisResults(results);

                            var atgs = retryOnException(() => Repository.GetAnalysisTaxonomicGroupsForProject(credentials.ProjectID, credentials).First());
                            Vocabulary.addAnalysisTaxonomicGroups(atgs);
                            CurrentStep = STATE_RESULTS_DONE;
                            break;
                        case STATE_RESULTS_DONE:
                            reportProgress(DiversityResources.RefreshVocabularyTask_State_LoadingProperties);
                            var properties = retryOnException(() => Repository.GetPropertiesForUser(credentials).First());
                            Vocabulary.addProperties(properties);

                            foreach (var p in properties)
                            {
                                Repository.DownloadPropertyValuesChunked(p)
                                    .ForEach(chunk => Vocabulary.addPropertyNames(chunk));
                            }
                            break;
                        default:
                            State[KEY_STATE] = STATE_ALL_DONE;
                            break;
                    }
                }       
            }
        }

        protected override void Cancel()
        {
            throw new NotImplementedException();
        }

        protected override void Cleanup(object arg)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<T> retryOnException<T>(Func<IEnumerable<T>> call)
        {
            if(call == null)
                return Enumerable.Empty<T>();


            IEnumerable<T> res = null;

            while (res == null) //In case the service call fails, we need to retry
            {
                try
                {
                    res = call();
                    if (res == null)
                        res = Enumerable.Empty<T>();
                }
                catch
                {
                    res = null;
                }
            }    
            return res;
        }
    }
}
