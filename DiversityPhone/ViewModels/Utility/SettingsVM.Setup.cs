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
using ReactiveUI;
using System.Reactive.Subjects;
using DiversityPhone.DiversityService;
using System.Collections.Generic;
using System.Reactive.Linq;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using System.Linq;


namespace DiversityPhone.ViewModels.Utility
{
    public partial class SettingsVM
    {
        public class SetupVM : ReactiveObject
        {
            SettingsVM _owner;
            IDisposable _saveDisposable;

            #region Setup Properties
            public enum Pivots
            {
                Login,
                Repository,
                Projects
            }

            private Pivots _CurrentPivot;
            public Pivots CurrentPivot
            {
                get
                {
                    return _CurrentPivot;
                }
                set
                {
                    this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value);
                }
            }

            private string _UserName
#if DEBUG
 = "Rollinge";
#else
        ;
#endif

            public string UserName
            {
                get
                {
                    return _UserName;
                }
                set
                {
                    this.RaiseAndSetIfChanged(x => x.UserName, ref _UserName, value);
                }
            }


            private string _Password
#if DEBUG
 = "Rolli#2-AI4@UB";
#else
        ;
#endif
            public string Password
            {
                get
                {
                    return _Password;
                }
                set
                {
                    this.RaiseAndSetIfChanged(x => x.Password, ref _Password, value);
                }
            }

            public IList<Repository> Databases { get { return (_Databases != null) ? _Databases.Value : null; } }
            private ObservableAsPropertyHelper<IList<Repository>> _Databases;


            private Repository _CurrentDB;
            public Repository CurrentDB
            {
                get
                {
                    return _CurrentDB;
                }
                set
                {
                    this.RaiseAndSetIfChanged(x => x.CurrentDB, ref _CurrentDB, value);
                }
            }


            public IList<Project> Projects { get { return (_Projects != null) ? _Projects.Value : null; } }
            private ObservableAsPropertyHelper<IList<Project>> _Projects;


            private Project _CurrentProject;
            public Project CurrentProject
            {
                get
                {
                    return _CurrentProject;
                }
                set
                {
                    this.RaiseAndSetIfChanged(x => x.CurrentProject, ref _CurrentProject, value);
                }
            }

            private IObservable<UserProfile> _Profile;

            public bool IsBusy { get { return _IsBusy.Value; } }
            private ObservableAsPropertyHelper<bool> _IsBusy;

            public string BusyMessage { get { return _BusyMessage.Value; } }
            private ISubject<string> _BusyMessageSubject = new Subject<string>();
            private ObservableAsPropertyHelper<string> _BusyMessage;

            public bool GettingProjects { get { return _GettingProjects.Value; } }
            private ObservableAsPropertyHelper<bool> _GettingProjects;

            public bool GettingRepositories { get { return _GettingRepositories.Value; } }
            private ObservableAsPropertyHelper<bool> _GettingRepositories;
            #endregion
           
            public IObservable<bool> CanSave { get { return canSave(); } }

            #region Async Operations
            ReactiveAsyncCommand getRepositories = new ReactiveAsyncCommand();
            ReactiveAsyncCommand getProjects = new ReactiveAsyncCommand();
            ReactiveAsyncCommand getUserInfo = new ReactiveAsyncCommand();
            ReactiveAsyncCommand finishSetup = new ReactiveAsyncCommand();
            #endregion

            private AppSettings createSettings()
            {
                var m = new AppSettings();
                var profile = _Profile.First();

                m.AgentName = profile.UserName;
                m.AgentURI = profile.AgentUri;
                m.CurrentProject = CurrentProject.ProjectID;
                m.CurrentProjectName = CurrentProject.DisplayText;
                m.HomeDB = CurrentDB.Database;
                m.HomeDBName = CurrentDB.DisplayName;
                m.Password = Password;
                m.UserName = UserName;

                return m;
            }

            private IObservable<bool> canSave()
            {
                var username = this.ObservableForProperty(x => x.UserName)
                                   .Select(change => !string.IsNullOrWhiteSpace(change.Value))
                                   .StartWith(false);
                var password = this.ObservableForProperty(x => x.Password)
                                   .Select(change => !string.IsNullOrEmpty(change.Value))
                                   .StartWith(false);
                var homeDB = this.ObservableForProperty(x => x.CurrentDB)
                                 .Select(change => change.Value != null)
                                 .StartWith(false);
                var project = this.ObservableForProperty(x => x.CurrentProject)
                                  .Select(change => change.Value != null)
                                  .StartWith(false);


                var profile = _Profile
                    .Select(p => p != null)
                    .StartWith(false);

                var settingsValid = Extensions.BooleanAnd(username, password, homeDB, project, profile);
                
                return settingsValid;
            }       

            public SetupVM(SettingsVM owner)
            {
                _owner = owner;
                _BusyMessage = _BusyMessageSubject
                    .ToProperty(this, x => x.BusyMessage);

                

                var creds =
                    Observable.CombineLatest(
                        this.ObservableForProperty(x => x.UserName),
                        this.ObservableForProperty(x => x.Password),
                        (user, pass) => new UserCredentials() { LoginName = user.Value, Password = pass.Value }
                    )                    
                    .DistinctUntilChanged();

                var credsWithRepo =
                    Observable.CombineLatest(
                    creds,
                    this.ObservableForProperty(x => x.CurrentDB).Where(repo => repo != null),
                    (usercreds, repo) =>
                    {
                        usercreds.Repository = repo.Value.Database;
                        return usercreds;
                    }).DistinctUntilChanged();                

                creds.Subscribe(login => getRepositories.Execute(login));
                _GettingRepositories =
                    getRepositories
                    .ItemsInflight
                    .Select(items => items > 0)
                    .ToProperty(this, x => x.GettingRepositories);
                _Databases = 
                    getRepositories
                    .RegisterAsyncFunction(login => _owner._DivSvc.GetRepositories(login as UserCredentials).Timeout(TimeSpan.FromSeconds(30),Observable.Return<IList<Repository>>(new List<Repository>())).First())
                    .ToProperty(this, x => x.Databases);
                _Databases
                    .Where(dbs => dbs.Any())
                    .Select(dbs => dbs.First())
                    .BindTo(this, x => x.CurrentDB);

                credsWithRepo.Subscribe(login => getProjects.Execute(login));
                _GettingProjects =
                    getProjects
                    .ItemsInflight
                    .Select(items => items > 0)
                    .ToProperty(this, x => x.GettingProjects);
                _Projects = 
                    getProjects
                    .RegisterAsyncFunction(login => _owner._DivSvc.GetProjectsForUser(login as UserCredentials).First())
                    .ToProperty(this, x => x.Projects);
                _Projects
                    .Where(projects => projects.Any())
                    .Select(projects => projects.First())
                    .BindTo(this, x => x.CurrentProject);

                creds.Subscribe(login => getUserInfo.Execute(login));
                var profile = 
                    getUserInfo
                    .RegisterAsyncFunction(login => _owner._DivSvc.GetUserInfo(login as UserCredentials).First())                    
                    .StartWith(new UserProfile[] { null })
                    .Replay(1); // Keep the last User Profile around
                profile.Connect();
                _Profile = profile;

                finishSetup
                    .RegisterAsyncAction(_ => finishSetupImpl());
                _IsBusy = 
                     finishSetup
                    .ItemsInflight
                    .Select(items => items > 0)
                    .ToProperty(this, x => x.IsBusy);

                _saveDisposable = _owner.Save
                    .Subscribe(_ => finishSetup.Execute(null));
                

                
            }

            private void finishSetupImpl()
            {
                _saveDisposable.Dispose();

                var settings = createSettings();
                var credentials = new UserCredentials(settings);               
                    
                var storageService = _owner._storage;
                var diversityService = _owner._DivSvc;

                _BusyMessageSubject.OnNext("Downloading Vocabulary");                
                var voc = diversityService.GetStandardVocabulary().First();
                var analysesObservable = diversityService.GetAnalysesForProject(CurrentProject, credentials);
                storageService.addTerms(voc);

                _BusyMessageSubject.OnNext("Downloading Analyses");
                var analyses = analysesObservable.First();
                var resultObservable = diversityService.GetAnalysisResultsForProject(CurrentProject, credentials);
                storageService.addAnalyses(analyses);

                _BusyMessageSubject.OnNext("Downloading Analysis Results");
                var results = resultObservable.First();
                var atgObservable = diversityService.GetAnalysisTaxonomicGroupsForProject(CurrentProject, credentials);

                storageService.addAnalysisResults(results);
                var atgs = atgObservable.First();

                storageService.addAnalysisTaxonomicGroups(atgs);

                _owner._settings.saveSettings(settings);
                _owner._ModelSubject.OnNext(settings);
            }
        }
    }
}
