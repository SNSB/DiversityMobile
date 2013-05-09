using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using DiversityPhone.Model;
using System;
using System.Linq;
using System.Reactive;
using DiversityPhone.Interface;
using ReactiveUI.Xaml;
using System.Reactive.Concurrency;


namespace DiversityPhone.ViewModels
{
    public class SetupVM : PageVMBase
    {
        private readonly TimeSpan NOTIFICATION_DURATION = TimeSpan.FromSeconds(5);


        readonly IDiversityServiceClient Repository;
        readonly ISettingsService Settings;
        readonly INotificationService Notifications;
        readonly IConnectivityService Connectivity;
        readonly ITaxonService Taxa;
        readonly IFieldDataService FieldData;


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

        private string NoRepo = DiversityResources.Setup_Item_PleaseChoose;

        public ListSelectionHelper<string> Databases { get; private set; }

        private Project NoProject = new Project() { DisplayText = DiversityResources.Setup_Item_PleaseChoose, ProjectID = -1 };

        public ListSelectionHelper<Project> Projects { get; private set; }

        private ObservableAsPropertyHelper<UserProfile> _Profile;

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

        private bool _UseGPS = false;

        public bool UseGPS
        {
            get { return _UseGPS; }
            set { _UseGPS = value; }
        }


        public ReactiveCommand RefreshVocabulary { get; private set; }

        public ReactiveCommand Save { get; private set; }
        #endregion

        #region Async Operations
        ReactiveAsyncCommand clearDatabase = new ReactiveAsyncCommand();
        #endregion

        private AppSettings createSettings()
        {
            var m = new AppSettings();
            var profile = _Profile.Value;

            m.AgentName = profile.UserName;
            m.AgentURI = profile.AgentUri;
            m.CurrentProject = Projects.SelectedItem.ProjectID;
            m.CurrentProjectName = Projects.SelectedItem.DisplayText;
            m.HomeDBName = Databases.SelectedItem;
            m.Password = Password;
            m.UserName = UserName;
            m.UseGPS = UseGPS;

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

        public SetupVM(
            [Dispatcher] IScheduler Dispatcher,
            IDiversityServiceClient Repository,
            ISettingsService Settings,
            INotificationService Notifications,
            IConnectivityService Connectivity,
            ITaxonService Taxa,
            IFieldDataService FieldData,
            Func<IRefreshVocabularyTask> refreshVocabluaryTaskFactory
            )
        {
            this.Repository = Repository;
            this.Settings = Settings;
            this.Notifications = Notifications;
            this.Connectivity = Connectivity;
            this.Taxa = Taxa;
            this.FieldData = FieldData;

            RefreshVocabulary = new ReactiveCommand();
            RefreshVocabulary
                .Subscribe(settings =>
                    {
                        if (settings == null || !(settings is AppSettings))
                            return;

                        var login = (settings as AppSettings).ToCreds();

                        refreshVocabluaryTaskFactory().Start(login)
                            .StartWith(Unit.Default)
                            .ObserveOn(Dispatcher)
                            .Subscribe(_ => { IsBusy = true; }, () =>
                            {
                                Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.REFRESH);
                                Messenger.SendMessage<Page>(Page.Home);
                            });
                    });


            clearDatabase.RegisterAsyncAction(_ =>
            {               

                if (Taxa == null || FieldData == null)
                {
#if DEBUG
                    throw new ArgumentNullException("services");
#else
                        return;
#endif
                }

                Taxa.clearTaxonLists();
                FieldData.clearDatabase();

                Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.INIT);
            });

            #region Setup
            Databases = new ListSelectionHelper<string>();
            Projects = new ListSelectionHelper<Project>();

            var creds =
                Observable.CombineLatest(
                    this.ObservableForProperty(x => x.UserName).Throttle(TimeSpan.FromMilliseconds(500)),
                    this.ObservableForProperty(x => x.Password).Throttle(TimeSpan.FromMilliseconds(500)),
                    (user, pass) => new UserCredentials() { LoginName = user.Value, Password = pass.Value }
                );

            creds
                .CheckConnectivity(Connectivity, Notifications)
                .SelectMany(login =>
                    {

                        return
                        Repository
                        .GetRepositories(login as UserCredentials)
                        .DisplayProgress(Notifications, DiversityResources.Setup_Info_GettingRepositories)
                        .HandleServiceErrors(Notifications, Messenger, Observable.Empty<IEnumerable<string>>())
                        .Where(repos =>
                            {
                                if (repos != null && repos.Any())
                                    return true;
                                else
                                {
                                    Notifications.showNotification(DiversityResources.Setup_Info_InvalidCredentials, NOTIFICATION_DURATION);
                                    return false;
                                }
                            });
                    })
                .Merge(creds.Select(_ => Enumerable.Empty<string>()))
                .ObserveOn(Dispatcher)
                .Select(repos =>
                    {
                        IList<string> repList = repos.ToList();
                        repList.Insert(0, NoRepo);
                        if (repList.Count > 1)
                        {
                            CurrentPivot = Pivots.Repository;
                        }

                        return repList;
                    })                
                .Subscribe(Databases);

            var credsWithRepo =
                Observable.CombineLatest(
                creds,
                Databases
                .Where(x => x != null && x != NoRepo),
                (usercreds, repo) =>
                {
                    usercreds.Repository = repo;
                    return usercreds;
                });

            credsWithRepo
                .CheckConnectivity(Connectivity, Notifications)
                .SelectMany(login =>
                    {
                        var gettingProjects = Notifications.showProgress(DiversityResources.Setup_Info_GettingProjects);
                        return
                        Repository
                        .GetProjectsForUser(login)
                        .Finally(gettingProjects.Dispose)
                        .HandleServiceErrors(Notifications, Messenger, Observable.Return(new List<Project>() as IList<Project>));
                    })

                .Merge(Databases.Select(_ => new List<Project>() as IList<Project>)) //Repo changed                
                .ObserveOn(Dispatcher)
                .Do(projects =>
                    {
                        projects.Insert(0, NoProject);
                        if (projects.Count > 1) { CurrentPivot = Pivots.Projects; }
                    })
                .Subscribe(Projects);


            _Profile = new ObservableAsPropertyHelper<UserProfile>(
                    credsWithRepo
                    .SelectMany(login =>
                        Repository
                        .GetUserInfo(login as UserCredentials)
                        .HandleServiceErrors(Notifications, Messenger, Observable.Return(null as UserProfile))
                    )
                    .Merge(credsWithRepo.Select(_ => null as UserProfile)),
                    _ => { }, null
                );
            #endregion


            Save = new ReactiveCommand(settingsValid().ObserveOn(Dispatcher));



            Settings.CurrentSettings()
                .Where(settings => settings == null)
                .Subscribe(_ =>
                    {
                        Messenger.SendMessage<DialogMessage>(new DialogMessage(DialogType.YesNo,
                        DiversityResources.Setup_Message_AllowGPS_Caption,
                        DiversityResources.Setup_Message_AllowGPS_Body,
                        (r) =>
                        {
                            UseGPS = r == DialogResult.OKYes;
                        }));
                        IsBusy = false;
                    });

            Save
                .Do(_ => clearDatabase.Execute(null))
                .Select(_ => createSettings())
                .Do(res => Settings.SaveSettings(res))
                .Merge(
                    Settings.CurrentSettings().Sample(this.OnActivation()).Where(s => s != null)                    
                )
                .Subscribe(RefreshVocabulary.Execute);


        }
    }

}
