using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace DiversityPhone.ViewModels
{
    public interface IValidateLogin : INotifyPropertyChanged
    {
        string UserName { get; set; }
        string Password { get; set; }
        IObservable<bool> IsLoginValid { get; }
        IObservable<AppSettings> ValidCredentials { get; }
    }

    public interface ISelectDatabase : IListSelector<string>
    {
        IObservable<bool> IsDatabaseSelected { get; }
        IObserver<AppSettings> ValidCredentials { get; }
        IObservable<AppSettings> CredentialsWithDatabase { get; }
    }

    public interface ISelectProject : IListSelector<Project>
    {
        IObservable<bool> IsProjectSelected { get; }
        IObserver<AppSettings> CredentialsWithDatabase { get; }
        IObservable<AppSettings> CredentialsWithProject { get; }
    }

    public interface IGetUserProfile
    {
        IObserver<AppSettings> CredentialsWithDatabase { get; }
        IObservable<bool> IsProfileValid { get; }
        IObservable<AppSettings> CredentialsWithProfile { get; }
    }

    public class SetupVM : PageVMBase
    {
        public IValidateLogin Login { get; private set; }
        public ISelectDatabase Database { get; private set; }
        public ISelectProject Project { get; private set; }
        public IGetUserProfile Profile { get; private set; }

        #region Setup Properties


        private bool _UseGPS = true;

        public bool UseGPS
        {
            get { return _UseGPS; }
            set { _UseGPS = value; }
        }

        public ReactiveCommand Save { get; private set; }
        #endregion



        public SetupVM(
            IValidateLogin Login,
            ISelectDatabase Database,
            ISelectProject Project,
            IGetUserProfile Profile,
            ISettingsService Settings
            )
        {
            this.Login = Login;
            this.Database = Database;
            this.Project = Project;
            this.Profile = Profile;

            Login.ValidCredentials
                .Subscribe(Database.ValidCredentials);

            Database.CredentialsWithDatabase
                .Subscribe(Project.CredentialsWithDatabase);

            Project.CredentialsWithProject
                .Subscribe(Profile.CredentialsWithDatabase);

            var canSave = Profile.IsProfileValid
                .Sample(Profile.CredentialsWithProfile.Where(p => p != null));

            Save = new ReactiveCommand(canSave, initialCondition: false);
            Profile.CredentialsWithProfile
                .Sample(Save)
                .Subscribe(s =>
                    {
                        s.UseGPS = this.UseGPS;
                        Settings.SaveSettings(s);
                        Messenger.SendMessage(Page.SetupVocabulary);
                    });
        }
    }

    public class LoginValidator : ListSelectionHelper<string>, IValidateLogin, ISelectDatabase
    {
        private string _UserName = "";
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

        public IObservable<bool> IsLoginValid
        {
            get { return _ValidCredentialsOut.Select(creds => creds != null).StartWith(false); }
        }

        IObservable<AppSettings> IValidateLogin.ValidCredentials
        {
            get
            {
                return _ValidCredentialsOut;
            }
        }


        public IObservable<bool> IsDatabaseSelected
        {
            get { return CredentialsWithDatabase.Select(creds => creds != null).StartWith(false); }
        }

        public IObserver<AppSettings> ValidCredentials
        {
            get;
            private set;
        }

        public IObservable<AppSettings> CredentialsWithDatabase
        {
            get;
            private set;
        }

        private IObservable<AppSettings> _ValidCredentialsOut;
        private string NoRepo = DiversityResources.Setup_Item_PleaseChoose;
        private IList<string> EmptyList = new List<string>();

        public LoginValidator(
            [Dispatcher] IScheduler Dispatcher,
            IMessageBus Messenger,
            IConnectivityService Connectivity,
            INotificationService Notifications,
            IDiversityServiceClient Repository
            )
            : base(Dispatcher)
        {
            //////////////// Login Validation

            var credentials =
                Observable.CombineLatest(
                this.ObservableForProperty(x => x.UserName).Throttle(TimeSpan.FromMilliseconds(500)),
                this.ObservableForProperty(x => x.Password).Throttle(TimeSpan.FromMilliseconds(500)),
                (user, pass) => new AppSettings() { UserName = user.Value, Password = pass.Value }
            ).Publish().PermaRef();

            var repositoryResults =
            credentials
            .CheckConnectivity(Connectivity, Notifications)
            .Select(s =>
            {
                return
                Repository
                .GetRepositories(s.ToCreds())
                .DisplayProgress(Notifications, DiversityResources.Setup_Info_ValidatingLogin)
                .HandleServiceErrors(Notifications, Messenger, Observable.Return<IEnumerable<string>>(null))
                .StartWith(new IEnumerable<string>[] { null })
                .Select(repos => new { Credentials = s, Repositories = repos ?? Enumerable.Empty<string>() });
            })
            .Switch()
            .Replay(1)
            .PermaRef();

            _ValidCredentialsOut = repositoryResults
                .Select(t => (t.Repositories.Any()) ? t.Credentials : null);

            ////////////// Database Selection

            repositoryResults
                .Select(t =>
                        Enumerable.Repeat(NoRepo, 1)
                        .Concat(t.Repositories)
                        .ToList() as IList<string>)
                .Subscribe(this.ItemsObserver);

            var validCredentialsIn = new Subject<AppSettings>();
            ValidCredentials = validCredentialsIn;

            CredentialsWithDatabase =
                validCredentialsIn.CombineLatest(this.SelectedItemObservable, (s, repo) =>
                {
                    if (repo != null && repo != NoRepo)
                    {
                        s.HomeDBName = repo;

                        return s;
                    }
                    else
                        return null;
                })
                .Replay(1)
                .PermaRef();
        }






    }

    public class ProjectSelector : ListSelectionHelper<Project>, ISelectProject
    {
        public IObservable<bool> IsProjectSelected
        {
            get { return LatestCredentialsWithProject.Select(c => c != null).StartWith(false); }
        }

        public IObserver<AppSettings> CredentialsWithDatabase { get; private set; }
        public IObservable<AppSettings> CredentialsWithProject { get; private set; }


        private Project NoProject = new Project() { DisplayText = DiversityResources.Setup_Item_PleaseChoose, ProjectID = -1 };
        private IList<Project> EmptyList = new List<Project>();
        private IObservable<AppSettings> LatestCredentialsWithProject;

        public ProjectSelector(
            [Dispatcher] IScheduler Dispatcher,
            IDiversityServiceClient Repository,
            INotificationService Notifications,
            IMessageBus Messenger
        )
            : base(Dispatcher)
        {
            var validCredentialsSubject = new Subject<AppSettings>();
            var validCredentials = validCredentialsSubject
                .Where(c => c != null);
            var projectsForCredentials =
                validCredentials
                .Select(s =>
                    Repository
                    .GetProjectsForUser(s.ToCreds())
                    .Select(l => l.AsEnumerable())
                    .DisplayProgress(Notifications, DiversityResources.Setup_Info_GettingProjects)
                    .HandleServiceErrors(Notifications, Messenger, Observable.Return(Enumerable.Empty<Project>()))
                    .StartWith(Enumerable.Empty<Project>())
                    .Select(projects => new { Credentials = s, Projects = projects ?? Enumerable.Empty<Project>() })
                    )
                .Switch()
                .Select(t => Enumerable.Repeat(NoProject, 1).Concat(t.Projects).ToList() as IList<Project>)
                .Subscribe(this.ItemsObserver);

            CredentialsWithDatabase = validCredentialsSubject;

            LatestCredentialsWithProject =
            this.SelectedItemObservable
            .CombineLatest(validCredentials, (project, s) =>
            {
                if (project != null && project != NoProject)
                {
                    s.CurrentProject = project.ProjectID;
                    s.CurrentProjectName = project.DisplayText;
                    return s;
                }
                else
                    return null;
            })
            .Publish()
            .PermaRef();

            CredentialsWithProject = LatestCredentialsWithProject.Where(creds => creds != null);
        }
    }

    public class ProfileLoader : ReactiveObject, IGetUserProfile
    {
        public IObservable<bool> IsProfileValid
        {
            get;
            private set;
        }

        public IObservable<AppSettings> CredentialsWithProfile
        {
            get
            {
                return LatestCredentialsWithProfile.Where(login => login != null);
            }
        }

        public IObserver<AppSettings> CredentialsWithDatabase { get; private set; }

        private IObservable<AppSettings> LatestCredentialsWithProfile;

        public ProfileLoader(
            IMessageBus Messenger,
            IConnectivityService Connectivity,
            INotificationService Notifications,
            IDiversityServiceClient Repository
            )
        {
            var credentialsWithDB = new Subject<AppSettings>();
            LatestCredentialsWithProfile =
            credentialsWithDB
            .SelectMany(s =>
            {
                return
                Repository
                .GetUserInfo(s.ToCreds())
                .DisplayProgress(Notifications, DiversityResources.Setup_Info_GettingProfile)
                .HandleServiceErrors(Notifications, Messenger, Observable.Return<UserProfile>(null))
                .Select(profile =>
                {
                    if (profile != null)
                    {
                        s.AgentName = profile.UserName;
                        s.AgentURI = profile.AgentUri;
                        return s;
                    }
                    else
                        return null;
                });
            })
            .Publish()
            .PermaRef();

            CredentialsWithDatabase = credentialsWithDB;

            IsProfileValid = LatestCredentialsWithProfile.Select(login => login != null).StartWith(false);
        }



    }


}
