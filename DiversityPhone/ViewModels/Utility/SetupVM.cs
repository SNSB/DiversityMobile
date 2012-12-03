using ReactiveUI;
using Svc = DiversityPhone.DiversityService;
using System.Collections.Generic;
using System.Reactive.Linq;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using System;
using DiversityPhone.Services;
using Funq;
using DiversityPhone.Services.BackgroundTasks;
using DiversityPhone.DiversityService;
using DiversityPhone.Messages;
using System.Reactive;


namespace DiversityPhone.ViewModels.Utility
{    
    public class SetupVM : ReactiveObject
    {
        private readonly TimeSpan NOTIFICATION_DURATION = TimeSpan.FromSeconds(3);


        IDiversityServiceClient Repository;
        ISettingsService Settings;
        IMessageBus Messenger;
        INotificationService Notifications;
        IConnectivityService Connectivity;


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

        private Svc.Repository NoRepo = new Repository() { Database = null, DisplayText = DiversityResources.Setup_Item_PleaseChoose };

        public ListSelectionHelper<Svc.Repository> Databases { get; private set; }

        private Svc.Project NoProject = new Project() { DisplayText = DiversityResources.Setup_Item_PleaseChoose, ProjectID = -1 };

        public ListSelectionHelper<Svc.Project> Projects { get; private set; }

        private ObservableAsPropertyHelper<Svc.UserProfile> _Profile;           

        private bool _IsBusy = true;

        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.IsBusy, ref _IsBusy, value);
            }
        }

        public ReactiveCommand RefreshVocabulary { get; private set; }

        public ReactiveCommand Save { get; private set; }
        #endregion        

        #region Async Operations
        ReactiveAsyncCommand clearDatabase = new ReactiveAsyncCommand();
        ReactiveAsyncCommand getRepositories = new ReactiveAsyncCommand();
        ReactiveAsyncCommand getProjects = new ReactiveAsyncCommand();
        ReactiveAsyncCommand getUserInfo = new ReactiveAsyncCommand();            
        #endregion

        private AppSettings createSettings()
        {
            var m = new AppSettings();
            var profile = _Profile.Value;

            m.AgentName = profile.UserName;
            m.AgentURI = profile.AgentUri;
            m.CurrentProject = Projects.SelectedItem.ProjectID;
            m.CurrentProjectName = Projects.SelectedItem.DisplayText;
            m.HomeDB = Databases.SelectedItem.Database;
            m.HomeDBName = Databases.SelectedItem.DisplayText;
            m.Password = Password;
            m.UserName = UserName;

            return m;
        }

        private IObservable<bool> settingsValid()
        {
            var username = this.ObservableForProperty(x => x.UserName)
                                .Select(change => !string.IsNullOrWhiteSpace(change.Value))
                                .StartWith(false);
            var password = this.ObservableForProperty(x => x.Password)
                                .Select(change => !string.IsNullOrEmpty(change.Value))
                                .StartWith(false);
            var homeDB = Databases
                            .Select(db => db != NoRepo)
                            .StartWith(false);
            var project = Projects
                                .Select(p => p != NoProject)
                                .StartWith(false);


            var profile = _Profile                   
                .Select(p => p != null)
                .StartWith(false);

            var settingsValid = Extensions.BooleanAnd(username, password, homeDB, project, profile);
                
            return settingsValid.DistinctUntilChanged();
        }       

        public SetupVM(Container ioc)
        {
            Repository = ioc.Resolve<IDiversityServiceClient>();
            Messenger = ioc.Resolve<IMessageBus>();
            Settings = ioc.Resolve<ISettingsService>();
            Notifications = ioc.Resolve<INotificationService>();
            Connectivity = ioc.Resolve<IConnectivityService>();

            RefreshVocabulary = new ReactiveCommand();
            RefreshVocabulary                
                .Subscribe(settings => 
                    {
                        if( settings == null || !(settings is AppSettings))
                            return;

                        var login = (settings as AppSettings).ToCreds();

                        RefreshVocabularyTask.Start(ioc, login )                            
                            .StartWith(Unit.Default)
                            .Subscribe(_ => { IsBusy = true; }, () =>
                            {
                                Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.CLEAN);
                                Messenger.SendMessage<Page>(Page.Home);
                            });
                    });
                                  

            clearDatabase.RegisterAsyncAction(_ =>
            {
                var taxa = ioc.Resolve<ITaxonService>();                
                var storage = ioc.Resolve<IFieldDataService>();

                if (taxa == null  || storage == null)
                {
#if DEBUG
                    throw new ArgumentNullException("services");
#else
                        return;
#endif
                }

                taxa.clearTaxonLists();                
                storage.clearDatabase();
            });

            #region Setup
            Databases = new ListSelectionHelper<Svc.Repository>();
            Projects = new ListSelectionHelper<Svc.Project>();

            var creds =
                Observable.CombineLatest(
                    this.ObservableForProperty(x => x.UserName),
                    this.ObservableForProperty(x => x.Password),
                    (user, pass) => new Svc.UserCredentials() { LoginName = user.Value, Password = pass.Value }
                );

            var credsWithRepo =
                Observable.CombineLatest(
                creds,
                Databases
                .Where(x => x != null),
                (usercreds, repo) =>
                {
                    usercreds.Repository = repo.Database;
                    return usercreds;
                });                

            creds  
                .CheckConnectivity(Connectivity, Notifications)
                .Subscribe(login => getRepositories.Execute(login));
            
            IDisposable gettingRepos = null;
            getRepositories
                .ItemsInflight
                .Select(items => items > 0)
                .Subscribe(busy =>
                    {
                        if (busy && gettingRepos == null)
                        {
                            gettingRepos = Notifications.showProgress(DiversityResources.Setup_Info_GettingRepositories);
                        }
                        else if (!busy && gettingRepos != null)
                        {
                            gettingRepos.Dispose();
                            gettingRepos = null;
                        }
                    });
                

            getRepositories                    
                .RegisterAsyncFunction(login => 
                    Repository                    
                    .GetRepositories(login as Svc.UserCredentials)
                    .HandleServiceErrors(Notifications, Messenger, Observable.Return<IList<Svc.Repository>>(new List<Svc.Repository>()))                    
                    .First())
                .Merge(creds.Select(_ => new List<Svc.Repository>() as IList<Svc.Repository>))
                .Do(repos => repos.Insert(0,NoRepo))                
                .Do(repos => 
                {
                    if (repos.Count > 1) 
                    {
                        CurrentPivot = Pivots.Repository; 
                    }
                })
                .Subscribe(Databases);                

            credsWithRepo.Subscribe(login => getProjects.Execute(login));

            IDisposable gettingProjects = null;            
            getProjects
                .ItemsInflight
                .Select(items => items > 0)
                .Subscribe(busy =>
                    {
                        if (busy && gettingProjects == null)
                        {
                            gettingProjects = Notifications.showProgress(DiversityResources.Setup_Info_GettingProjects);
                        }
                        else if (!busy && gettingProjects != null)
                        {
                            gettingProjects.Dispose();
                            gettingProjects = null;
                        }
                            
                    });
                
            getProjects
                .RegisterAsyncFunction(login => 
                    Repository
                    .GetProjectsForUser(login as Svc.UserCredentials)
                    .HandleServiceErrors(Notifications, Messenger, Observable.Return(new List<Svc.Project>() as IList<Svc.Project>) )
                    .First())                     
                .Merge(Databases.Select(_ => new List<Svc.Project>() as IList<Svc.Project>)) //Repo changed
                .Do(projects => projects.Insert(0, NoProject ))
                .ObserveOnDispatcher()
                .Do(projects =>
                {
                    if (projects.Count > 1) { CurrentPivot = Pivots.Projects; }
                })
                .Subscribe(Projects);

            credsWithRepo.Subscribe(login => getUserInfo.Execute(login));
            _Profile = new ObservableAsPropertyHelper<Svc.UserProfile>(
                    getUserInfo
                    .RegisterAsyncFunction(login => 
                        Repository
                        .GetUserInfo(login as Svc.UserCredentials)
                        .HandleServiceErrors(Notifications, Messenger, Observable.Return(null as UserProfile))
                        .First()),
                    _ => { }, null
                );            
            #endregion


            Save = new ReactiveCommand(settingsValid().ObserveOnDispatcher());

            var existingSettings = Observable.Return(Settings.getSettings());

            existingSettings
                .Where(settings => settings == null)
                .Subscribe(_ => { IsBusy = false; });

            Save
                .Do(_ => clearDatabase.Execute(null))
                .Select(_ => createSettings())
                .Merge(existingSettings.Where(settings => settings != null)) // just refresh
                .Do(res => Settings.saveSettings(res))
                .Subscribe(RefreshVocabulary.Execute);

            
        }
    }
    
}
