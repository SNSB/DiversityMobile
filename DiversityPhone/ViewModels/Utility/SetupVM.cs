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


namespace DiversityPhone.ViewModels.Utility
{    
    public class SetupVM : ReactiveObject
    {
        IDiversityServiceClient Repository;
        ISettingsService Settings;
        IMessageBus Messenger;


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

        public bool GettingProjects { get { return _GettingProjects.Value; } }
        private ObservableAsPropertyHelper<bool> _GettingProjects;

        public bool GettingRepositories { get { return _GettingRepositories.Value; } }
        private ObservableAsPropertyHelper<bool> _GettingRepositories;

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        public string BusyMessage { get { return _BusyMessage.Value; } }
        private ObservableAsPropertyHelper<string> _BusyMessage;

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
            var background = ioc.Resolve<IBackgroundService>();
            var refreshVocabularyTask = background.getTaskObject<RefreshVocabularyTask>();

            RefreshVocabulary = new ReactiveCommand();
            RefreshVocabulary
                .Subscribe(_ => background.startTask<RefreshVocabularyTask>(Settings.getSettings().ToCreds()));

            _IsBusy = this.ObservableToProperty(refreshVocabularyTask.BusyObservable.ObserveOnDispatcher(), x => x.IsBusy);

            _BusyMessage = this.ObservableToProperty(
                refreshVocabularyTask.AsyncProgressMessages.ObserveOnDispatcher(), x => x.BusyMessage);

            refreshVocabularyTask.AsyncCompletedNotification
                .Take(1)
                .Subscribe(_ =>
                    {
                        Messenger.SendMessage<EventMessage>(new EventMessage(), MessageContracts.CLEAN);
                        Messenger.SendMessage<Page>(Page.Home);
                    });

            clearDatabase.RegisterAsyncAction(_ =>
            {
                var taxa = ioc.Resolve<ITaxonService>();
                var vocabulary = ioc.Resolve<IVocabularyService>();
                var storage = ioc.Resolve<IFieldDataService>();

                if (taxa == null || vocabulary == null || storage == null)
                {
#if DEBUG
                    throw new ArgumentNullException("services");
#else
                        return;
#endif
                }

                taxa.clearTaxonLists();
                vocabulary.clearVocabulary();
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
                .Subscribe(login => getRepositories.Execute(login));
            _GettingRepositories = this.ObservableToProperty(
                getRepositories
                .ItemsInflight
                .Select(items => items > 0)
                .ObserveOnDispatcher(),
                x => x.GettingRepositories);

            getRepositories                    
                .RegisterAsyncFunction(login => 
                    Repository
                    .GetRepositories(login as Svc.UserCredentials)
                    .OnServiceUnavailable(() => {notifySvcUnavailable(); return new List<Svc.Repository>();})                    
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
            _GettingProjects = this.ObservableToProperty(
                getProjects
                .ItemsInflight
                .Select(items => items > 0)
                .ObserveOnDispatcher(),
                x => x.GettingProjects);
                
            getProjects
                .RegisterAsyncFunction(login => 
                    Repository
                    .GetProjectsForUser(login as Svc.UserCredentials)
                    .OnServiceUnavailable(() => { notifySvcUnavailable(); return new List<Svc.Project>() as IList<Svc.Project>;} )
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
                        .OnServiceUnavailable(() => { notifySvcUnavailable(); return null;})
                        .First()),
                    _ => { }, null
                );            
            #endregion


            Save = new ReactiveCommand(settingsValid().ObserveOnDispatcher());

            Save
                .Do(_ => clearDatabase.Execute(null))
                .Select(_ => createSettings())
                .Merge(Observable.Return(Settings.getSettings()).Where(settings => settings != null)) // just refresh
                .Do(res => Settings.saveSettings(res))
                .Subscribe(RefreshVocabulary.Execute);

            
        }

        private void notifySvcUnavailable()
        {
            Messenger.SendMessage(
                new DialogMessage( Messages.DialogType.OK,
                    DiversityResources.Message_SorryHeader,
                    DiversityResources.Message_ServiceUnavailable_Body));
        }
            
    }
    
}
