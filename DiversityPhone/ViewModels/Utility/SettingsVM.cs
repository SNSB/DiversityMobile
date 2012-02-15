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

namespace DiversityPhone.ViewModels.Utility
{
    public class SettingsVM : PageViewModel
    {
        SettingsService _settings;
        IDiversityServiceClient _DivSvc;
        IOfflineStorage _storage;

        public bool IsFirstSetup { get { return _IsFirstSetup.Value; } }
        private ObservableAsPropertyHelper<bool> _IsFirstSetup;



        #region FirstSetup

        public bool InProgress { get { return _InProgress.Value; } }
        private ObservableAsPropertyHelper<bool> _InProgress;
        

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


        public bool EnableRepository { get { return _EnableRepository.Value; } }
        private ObservableAsPropertyHelper<bool> _EnableRepository;


        public bool EnableProjects { get { return _EnableProjects.Value; } }
        private ObservableAsPropertyHelper<bool> _EnableProjects;
        
        

        private string _UserName;

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


        private string _Password;
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


        private bool _SavePassword;

        public bool SavePassword
        {
            get
            {
                return _SavePassword;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SavePassword, ref _SavePassword, value);
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
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelBackingStore = new Subject<AppSettings>();

        private IDisposable test1, test2;
        
        public SettingsVM(SettingsService set, IDiversityServiceClient divsvc, IOfflineStorage storage)            
        {
            _settings = set;          
            _DivSvc = divsvc;
            _storage = storage;     

            _Model =_ModelBackingStore
                .ToProperty(this, x => x.Model);           

            _IsFirstSetup =
                _Model
                .Select( x => x.UserName == null)
                .ToProperty(this, x => x.IsFirstSetup);

            Reset = new ReactiveCommand(_IsFirstSetup.Select(x => !x));
            Save = new ReactiveCommand(CanSave());

            _IsFirstSetup
                .Where(x => x)
                .Subscribe(_ => OnSetup());

            _IsFirstSetup
                .Where(x => !x)
                .Subscribe(_ => OnSettings());


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
                  _ModelBackingStore.OnNext(m);
              });

            Messenger.RegisterMessageSource(
                Save
                .Where(_ => !IsFirstSetup)
                .Select(_ => new NavigationMessage(Services.Page.Previous))
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

        private void OnSetup()
        {
            var creds =
            Observable.CombineLatest(
                this.ObservableForProperty(x => x.UserName),
                this.ObservableForProperty(x => x.Password),
                (user, pass) => new UserCredentials() { UserName = user.Value, Password = pass.Value }
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
            _InProgress =
                Observable.Merge(
                    gettingDBs,
                    gettingProjects
                )
                .ToProperty(this, x => x.InProgress);

            _Databases = creds                
                .Do(_ => gettingDBs.OnNext(true))
                .SelectMany(login => _DivSvc.GetRepositories(login))
                .Do(_ => gettingDBs.OnNext(false))
                .ToProperty(this, x => x.Databases);
            _Databases
                .Select(dbs => dbs.FirstOrDefault())
                .BindTo(this, x => x.CurrentDB);

            _Projects = credsWithRepo               
               .Do(_ => gettingProjects.OnNext(true))
               .SelectMany(login => _DivSvc.GetProjectsForUser(login))
               .Do(_ => gettingProjects.OnNext(false))
               .ToProperty(this, x => x.Projects);
            _Projects
                .Select(projects => projects.FirstOrDefault())
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
            m.Password = (SavePassword)? Password : null;
            m.SavePassword = SavePassword;
            m.UserName = UserName;

            return m;
        }

        private IObservable<bool> CanSave()
        {
            var username = this.ObservableForProperty(x => x.UserName)
                               .Select(change => !string.IsNullOrWhiteSpace(change.Value))
                               .StartWith(false);
            var password = this.ObservableForProperty(x => x.Password)
                               .Select(change => !string.IsNullOrEmpty(change.Value));
            var homeDB = this.ObservableForProperty(x => x.CurrentDB)
                             .Select(change => change.Value != null);
            var project = this.ObservableForProperty(x => x.CurrentProject)
                              .Select(change => change.Value != null);

            var profile = _Profile.Select(p => p != null);

            var settingsValid = Extensions.BooleanAnd(username,password,homeDB,project, profile);

            //Can Save if the settings are valid (on setup)
            //Or always (on non-setup)
            return settingsValid.BooleanOr(_IsFirstSetup.Select(x => !x)).StartWith(false).Replay(1);
        }
    }
}
