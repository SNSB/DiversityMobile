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
using DiversityPhone.Services;
using DiversityPhone.Model;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using ReactiveUI;
using System.Linq;
using System.Collections.Generic;
using DiversityPhone.DiversityService;
using System.Reactive.Subjects;
using DiversityPhone.Messages;
using System.Reactive.Disposables;

namespace DiversityPhone.ViewModels.Utility
{
    public class SettingsVM : PageViewModel
    {
        ISettingsService _settings;
        IDiversityServiceClient _DivSvc;
        IOfflineStorage _storage;

        public bool IsFirstSetup { get { return _IsFirstSetup.Value; } }
        private ObservableAsPropertyHelper<bool> _IsFirstSetup;

        public bool InProgress { get { return _InProgress.Value; } }        
        private ISubject<bool> _InProgressBackingStore = new Subject<bool>();
        private ObservableAsPropertyHelper<bool> _InProgress;


        #region FirstSetup

        
        

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
        
        private ISubject<UserProfile> _Profile = new ReplaySubject<UserProfile>(1);
        

        #endregion


        private bool _UseGPS;

        public bool UseGPS
        {
            get
            {
                return _UseGPS;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.UseGPS,ref _UseGPS, value);
            }
        }             
        public ReactiveCommand Save { get; private set; }

        public ReactiveCommand Reset { get; private set; }

        public ReactiveCommand ManageTaxa { get; private set; }
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelBackingStore = new Subject<AppSettings>();       
        
        public SettingsVM(ISettingsService set, IDiversityServiceClient divsvc, IOfflineStorage storage)            
        {
            _settings = set;          
            _DivSvc = divsvc;
            _storage = storage;     

            _Model =_ModelBackingStore                
                .ToProperty(this, x => x.Model);            

            _ModelBackingStore
                .Select(m => m.UseGPS)
                .BindTo(this, x => x.UseGPS);

            _IsFirstSetup =
                _ModelBackingStore
                .Select( x => x.UserName == null)                
                .ToProperty(this, x => x.IsFirstSetup);

            Reset = new ReactiveCommand(_IsFirstSetup.Select(x => !x));
            Save = new ReactiveCommand(CanSave());         

            _IsFirstSetup
                .Where(x => x)
                .Take(1)
                .Subscribe(_ => OnSetupOnce());

            _IsFirstSetup
                .Where(x => !x)                
                .Subscribe(_ => OnSettings());

            _InProgress = _InProgressBackingStore
                .ToProperty(this, x => x.InProgress);

            

            Save.Select(_ => Model)
              .Select(m =>
                  {
                      if (IsFirstSetup)
                          return updateModel(m);
                      else
                      {
                          m.UseGPS = UseGPS;
                          return m;
                      }
                  })
              .Subscribe(m =>
              {
                  _settings.saveSettings(m);
                  _ModelBackingStore.OnNext(m.Clone()); //Clone, so Property will get updated
              });
                                   
            Messenger.RegisterMessageSource(
                Save
                .Where(_ => !IsFirstSetup)
                .Select(_ => new NavigationMessage(Services.Page.Previous))
                );

            ManageTaxa = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ManageTaxa
                .Select(_ => new NavigationMessage(Services.Page.TaxonManagement))
                );


            _ModelBackingStore.OnNext(_settings.getSettings());            
        }        

        private void OnSettings()
        {
            Messenger.RegisterMessageSource(
                Reset
                .Take(1)
                .Select(_ => new DialogMessage(
                    Messages.DialogType.YesNo, 
                    "Are you sure?", 
                    "All diversity data you have not already uploaded will be lost!",
                    res =>
                    {
                        if(res == DialogResult.OKYes)
                            OnReset();
                    }
                    )));
                
        }

        private void OnReset()
        {
            _storage.clearDatabase();
            _settings.saveSettings(new AppSettings());
            _ModelBackingStore.OnNext(new AppSettings());
        }

        private void OnSetupOnce()
        {
            var creds =
            Observable.CombineLatest(
                this.ObservableForProperty(x => x.UserName),
                this.ObservableForProperty(x => x.Password),
                (user, pass) => new UserCredentials() { LoginName = user.Value, Password = pass.Value }
                ).DistinctUntilChanged();

            var credsWithRepo =
                Observable.CombineLatest(
                creds,
                this.ObservableForProperty(x => x.CurrentDB).Where(repo => repo != null),
                (usercreds, repo) =>
                {
                    usercreds.Repository = repo.Value.Database;
                    return usercreds;
                }).DistinctUntilChanged();

            var gettingDBs = new Subject<bool>();
            var gettingProjects = new Subject<bool>();
            Observable.Merge(
                gettingDBs,
                gettingProjects
            )
            .Subscribe(_InProgressBackingStore);

            _Databases = creds                
                .Do(_ => gettingDBs.OnNext(true))
                .SelectMany(login => _DivSvc.GetRepositories(login))                
                .Do(_ => gettingDBs.OnNext(false))
                .ToProperty(this, x => x.Databases);
            _Databases
                .Where(dbs => dbs.Any())
                .Select(dbs => dbs.First())
                .BindTo(this, x => x.CurrentDB);

            _Projects = credsWithRepo               
               .Do(_ => gettingProjects.OnNext(true))
               .SelectMany(login => _DivSvc.GetProjectsForUser(login))               
               .Do(_ => gettingProjects.OnNext(false))
               .ToProperty(this, x => x.Projects);
            _Projects
                .Where(projects => projects.Any())
                .Select(projects => projects.First())
                .BindTo(this, x => x.CurrentProject);

            creds
                .SelectMany(login => _DivSvc.GetUserInfo(login)) 
                .StartWith(new UserProfile[]{null})
                .Subscribe(_Profile);                
        }

        private AppSettings updateModel(AppSettings m)
        {
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

        private IObservable<bool> CanSave()
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

            var settingsValid = Extensions.BooleanAnd(username,password,homeDB,project, profile);

            //Can Save if the settings are valid (on setup)
            //Or always (on non-setup)
            return settingsValid.BooleanOr(_IsFirstSetup.Select(x => !x)).StartWith(false);
        }
    }
}
