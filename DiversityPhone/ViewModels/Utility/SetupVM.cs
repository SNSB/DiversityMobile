namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public class SetupVM : PageVMBase
    {
        private readonly IDiversityServiceClient Repository;
        private readonly ISettingsService Settings;

        private IEnumerator<Settings> LatestLogin, LatestLoginWithRepo, LatestLoginWithProfile;

        public IReactiveCommand ShowLogin { get; set; }

        public ReactiveAsyncCommand GetRepositories { get; private set; }

        public ReactiveAsyncCommand GetProjects { get; private set; }

        public ReactiveAsyncCommand GetProfile { get; private set; }

        public ReactiveAsyncCommand Save { get; private set; }

        private string NoRepo = DiversityResources.Setup_Item_PleaseChoose;

        public IListSelector<string> Database { get; private set; }

        private Project NoProject = new Project() { DisplayText = DiversityResources.Setup_Item_PleaseChoose, ProjectID = -1 };

        public IListSelector<Project> Project { get; private set; }

        private string _UserName;

        public string UserName
        {
            get { return _UserName; }
            set { this.RaiseAndSetIfChanged(x => x.UserName, ref _UserName, value); }
        }

        private string _Password;

        public string Password
        {
            get { return _Password; }
            set { this.RaiseAndSetIfChanged(x => x.Password, ref _Password, value); }
        }

        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;

        public bool IsOnlineAvailable
        {
            get { return _IsOnlineAvailable.Value; }
        }

        private bool _UseGPS = true;

        public bool UseGPS
        {
            get { return _UseGPS; }
            set { _UseGPS = value; }
        }

        private IObservable<Tuple<Settings, IList<string>>> GetRepositoriesObservable(object _)
        {
            var user = UserName;
            var pass = Password;
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                // Invalid Argument
                return Observable.Empty<Tuple<Settings, IList<string>>>();
            }

            var settings = new Settings()
            {
                UserName = user,
                Password = pass
            };

            return Repository.GetRepositories(settings.ToCreds())
                .Select(repos => repos.ToList() as IList<string>)
                .Do(list => list.Insert(0, NoRepo))
                .Select(list => Tuple.Create(settings, list));
        }

        private IObservable<Tuple<Settings, IList<Project>>> GetProjectsObservable(object _)
        {
            var repo = Database.SelectedItem;
            var login = LatestLogin.NextOrDefault();

            if (!string.IsNullOrWhiteSpace(repo) && repo != NoRepo && login != null)
            {
                login.HomeDBName = repo;
                return Repository.GetProjectsForUser(login.ToCreds())
                    .Do(list => list.Insert(0, NoProject))
                    .Select(projects => Tuple.Create(login, projects));
            }
            else
            {
                return Observable.Empty<Tuple<Settings, IList<Project>>>();
            }
        }

        private IObservable<Settings> GetProfileObservable(object _)
        {
            var project = Project.SelectedItem;
            var login = LatestLoginWithRepo.NextOrDefault();

            if (project != null && project != NoProject && login != null)
            {
                login.CurrentProject = project.ProjectID;
                login.CurrentProjectName = project.DisplayText;
                return Repository.GetUserInfo(login.ToCreds())
                    .Select(profile =>
                    {
                        login.AgentName = profile.UserName;
                        login.AgentURI = profile.AgentUri;
                        return login;
                    });
            }
            else
            {
                return Observable.Empty<Settings>();
            }
        }

        private void SetLogin(Settings settings)
        {
            if (settings != null)
            {
                this.UserName = settings.UserName;
                this.Password = settings.Password;
            }
        }

        private IObservable<Unit> SaveSettings(object _)
        {
            var settings = LatestLoginWithProfile.NextOrDefault();

            if (settings != null)
            {
                settings.UseGPS = this.UseGPS;
                return Observable.Start(() => Settings.SaveSettings(settings));
            }
            else
            {
                return Observable.Empty<Unit>();
            }
        }

        public SetupVM(
            ISettingsService Settings,
            IConnectivityService Connectivity,
            [Dispatcher] IScheduler Dispatcher,
            IDiversityServiceClient Repository
            )
        {
            this.Repository = Repository;
            this.Settings = Settings;

            // On First Page Visit (App Launch)
            // If There already is a configuration (Settings)
            // Go To Home Page
            this.FirstActivation()
                .SelectMany(_ => Settings.CurrentSettings())
                .Select(x => (x != null) ? Page.Home : Page.SetupWelcome)
                .ToMessage(Messenger);

            _IsOnlineAvailable = this.ObservableToProperty(Connectivity.WifiAvailable(), x => x.IsOnlineAvailable, false, Dispatcher);

            // Show current login data in case of Reset
            Settings.SettingsObservable()
                .Subscribe(SetLogin);

            // Command To begin Setup
            this.ShowLogin = new ReactiveCommand();
            ShowLogin.Select(_ => Page.SetupLogin)
                .ToMessage(Messenger);

            // Command Condition
            var userPassAndWifi =
                Observable.CombineLatest(
                Connectivity.WifiAvailable(),
                this.WhenAny(x => x.UserName, x => x.GetValue()).Select(string.IsNullOrWhiteSpace),
                this.WhenAny(x => x.Password, x => x.GetValue()).Select(string.IsNullOrWhiteSpace),
                (wifi, a, b) => wifi & !(a | b));

            // Command and Errorhandling
            this.GetRepositories = new ReactiveAsyncCommand(userPassAndWifi);
            GetRepositories.ShowInFlightNotification(Notifications, DiversityResources.Setup_Info_ValidatingLogin);
            GetRepositories.ThrownExceptions
                .ShowServiceErrorNotifications(Notifications)
                .ShowErrorNotifications(Notifications)
                .Subscribe();
            var loginAndRepo = GetRepositories.RegisterAsyncObservable(GetRepositoriesObservable).Publish().PermaRef();

            // Page Navigation if Login Successful
            // i.e. Any repositories have been returned
            loginAndRepo
                .Snd()
                .Subscribe(NavigateOrNotifyInvalidCredentials);

            // Repo Selection
            this.Database = new ListSelectionHelper<string>(Dispatcher);
            loginAndRepo
                .Select(t => t.Item2)
                .Merge(EmptyProjectsOnLoginStart())
                .Subscribe(Database.ItemsObserver);

            // Settings Propagation
            LatestLogin = loginAndRepo
               .Fst()
               .MostRecent(null)
               .GetEnumerator();

            // Command Condition
            var repoSelected = Database.SelectedItemObservable
                .Select(repo => repo != NoRepo)
                .AndNoItemsInFlight(GetRepositories);

            // Command and Errorhandling
            this.GetProjects = new ReactiveAsyncCommand(repoSelected);
            GetProjects.ShowInFlightNotification(Notifications, DiversityResources.Setup_Info_GettingProjects);
            GetProjects.ThrownExceptions
                .ShowServiceErrorNotifications(Notifications)
                .ShowErrorNotifications(Notifications)
                .Subscribe();
            var loginAndProjects = GetProjects.RegisterAsyncObservable(GetProjectsObservable).Publish().PermaRef();

            // Page Navigation
            loginAndProjects
                .Select(_ => Page.SetupProject)
                .ToMessage(Messenger);

            // Project Selection
            Project = new ListSelectionHelper<Project>(Dispatcher);
            loginAndProjects
                .Snd()
                .Merge(
                   EmptyReposOnRepoChange()
                   )
                   .Subscribe(Project.ItemsObserver);

            // Settings Propagation
            LatestLoginWithRepo = loginAndProjects
                .Fst()
                .MostRecent(null)
                .GetEnumerator();

            // Command Condition
            var projectSelected = Project.SelectedItemObservable
                .Select(p => p != NoProject)
                .AndNoItemsInFlight(GetProjects);

            // Command and Errorhandling
            this.GetProfile = new ReactiveAsyncCommand(projectSelected);
            GetProfile.ShowInFlightNotification(Notifications, DiversityResources.Setup_Info_GettingProfile);
            GetProfile.ThrownExceptions
                .ShowServiceErrorNotifications(Notifications)
                .ShowErrorNotifications(Notifications)
                .Subscribe();
            var loginWithProfile = GetProfile.RegisterAsyncObservable(GetProfileObservable).Publish().PermaRef();

            // Page Navigation
            loginWithProfile
                .Select(_ => Page.SetupGPS)
                .ToMessage(Messenger);

            // Settings Propagation
            LatestLoginWithProfile = loginWithProfile
                .MostRecent(null)
                .GetEnumerator();

            // Command And Page Navigation
            this.Save = new ReactiveAsyncCommand();
            Save.RegisterAsyncObservable(SaveSettings)
                .Select(_ => Page.SetupVocabulary)
                .ToMessage(Messenger);
        }

        private void NavigateOrNotifyInvalidCredentials(IList<string> repos)
        {
            // Don't count the "NoRepo" Entry
            if (repos != null && repos.Count > 1)
            {
                // Navigate Forward to Database Page
                Messenger.SendMessage(Page.SetupDatabase);
            }
            else
            {
                // Notify user of invalid Credentials
                Notifications.showNotification(DiversityResources.Setup_Info_InvalidCredentials);
            }
        }

        private IObservable<IList<Project>> EmptyReposOnRepoChange()
        {
            return GetProjects.AsyncStartedNotification
                               .Select(_ => new List<Project>() as IList<Project>)
                               .Do(l => l.Add(NoProject));
        }

        private IObservable<IList<string>> EmptyProjectsOnLoginStart()
        {
            return GetRepositories.AsyncStartedNotification
                                .Select(_ => new List<string>() as IList<string>)
                                .Do(l => l.Add(NoRepo));
        }
    }
}