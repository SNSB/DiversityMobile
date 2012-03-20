using ReactiveUI;
using System.Reactive.Subjects;
using Svc = DiversityPhone.DiversityService;
using System.Collections.Generic;
using System.Reactive.Linq;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using System.Linq;
using System;


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

            public ListSelectionHelper<Svc.Repository> Databases { get; private set; }            

            public ListSelectionHelper<Svc.Project> Projects { get; private set; }            

            private IObservable<Svc.UserProfile> _Profile;           

            public bool GettingProjects { get { return _GettingProjects.Value; } }
            private ObservableAsPropertyHelper<bool> _GettingProjects;

            public bool GettingRepositories { get { return _GettingRepositories.Value; } }
            private ObservableAsPropertyHelper<bool> _GettingRepositories;
            #endregion

            public IObservable<AppSettings> Result { get; private set; }

            #region Async Operations
            ReactiveAsyncCommand getRepositories = new ReactiveAsyncCommand();
            ReactiveAsyncCommand getProjects = new ReactiveAsyncCommand();
            ReactiveAsyncCommand getUserInfo = new ReactiveAsyncCommand();            
            #endregion

            private AppSettings createSettings()
            {
                var m = new AppSettings();
                var profile = _Profile.First();

                m.AgentName = profile.UserName;
                m.AgentURI = profile.AgentUri;
                m.CurrentProject = Projects.SelectedItem.ProjectID;
                m.CurrentProjectName = Projects.SelectedItem.DisplayText;
                m.HomeDB = Databases.SelectedItem.Database;
                m.HomeDBName = Databases.SelectedItem.DisplayName;
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
                var homeDB = Databases
                                .Select(db => db.Database != null)
                                .StartWith(false);
                var project = Projects
                                  .Select(p => p.DisplayText != null)
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

                Databases = new ListSelectionHelper<Svc.Repository>();
                Projects = new ListSelectionHelper<Svc.Project>();

                var creds =
                    Observable.CombineLatest(
                        this.ObservableForProperty(x => x.UserName),
                        this.ObservableForProperty(x => x.Password),
                        (user, pass) => new Svc.UserCredentials() { LoginName = user.Value, Password = pass.Value }
                    )                    
                    .DistinctUntilChanged();

                var credsWithRepo =
                    Observable.CombineLatest(
                    creds,
                    Databases
                    .Where(x => x != null),
                    (usercreds, repo) =>
                    {
                        usercreds.Repository = repo.Database;
                        return usercreds;
                    }).DistinctUntilChanged();                

                creds.Subscribe(login => getRepositories.Execute(login));
                _GettingRepositories =
                    getRepositories
                    .ItemsInflight
                    .Select(items => items > 0)
                    .ToProperty(this, x => x.GettingRepositories);

                getRepositories
                    .RegisterAsyncFunction(login => _owner._DivSvc.GetRepositories(login as Svc.UserCredentials).Timeout(TimeSpan.FromSeconds(30), Observable.Return<IList<Svc.Repository>>(new List<Svc.Repository>())).First())
                    .Subscribe(Databases);                

                credsWithRepo.Subscribe(login => getProjects.Execute(login));
                _GettingProjects =
                    getProjects
                    .ItemsInflight
                    .Select(items => items > 0)
                    .ToProperty(this, x => x.GettingProjects);
                
                getProjects
                    .RegisterAsyncFunction(login => _owner._DivSvc.GetProjectsForUser(login as Svc.UserCredentials).First())
                    .Subscribe(Projects);                

                creds.Subscribe(login => getUserInfo.Execute(login));
                var profile = 
                    getUserInfo
                    .RegisterAsyncFunction(login => _owner._DivSvc.GetUserInfo(login as Svc.UserCredentials).First())
                    .StartWith(new Svc.UserProfile[] { null })
                    .Replay(1); // Keep the last User Profile around
                profile.Connect();
                _Profile = profile;         
            }           
        }
    }
}
