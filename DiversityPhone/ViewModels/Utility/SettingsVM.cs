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
using System.Collections.Generic;
using DiversityPhone.DiversityService;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels.Utility
{
    public class SettingsVM : PageViewModel
    {
        SettingsService _settings;

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

        public IList<string> Databases { get { return _Databases.Value; } }
        private ObservableAsPropertyHelper<IList<DiversityService.>> _Databases;


        private string _CurrentDB;
        public string CurrentDB
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


        public IList<Project> Projects { get { return _Projects.Value; } }
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
        private Subject<AppSettings> _ModelBackingStore = new Subject<AppSettings>();
        
        
        public SettingsVM(SettingsService set)            
        {
            _settings = set;            
            Save = new ReactiveCommand(CanSave());            

            _Model =_ModelBackingStore
                .ToProperty(this, x => x.Model);           

            _IsFirstSetup =
                _Model
                .Select( x => x.UserName == null)
                .ToProperty(this, x => x.IsFirstSetup);

            _IsFirstSetup
                .Where(x => x)
                .Do(_ => OnSetup());

            _IsFirstSetup
                .Where(x => !x)
                .Do(_ => OnSettings());

            _ModelBackingStore.OnNext(_settings.getSettings());            
        }

        private void OnSettings()
        {
 	        Save = new ReactiveCommand();

            Messenger.RegisterMessageSource(
                Save
                .Do(_ => Model.UseGPS = UseGPS)
                .Do(_ => _settings.saveSettings(Model))
                .Select(_ => Messages.Message.NavigateBack)
            );
        }

        private void OnSetup()
        {
            var onPivot = this.ObservableForProperty(x => x.CurrentPivot)
                              .Where(p => p.Value != Pivots.Login);
            
                
        }

        private IObservable<bool> CanSave()
        {
            var username = this.ObservableForProperty(x => x.UserName)
                               .Select(change => !string.IsNullOrWhiteSpace(change.Value));
            var password = this.ObservableForProperty(x => x.Password)
                               .Select(change => string.IsNullOrEmpty(change.Value));
            var homeDB = this.ObservableForProperty(x => x.CurrentDB)
                             .Select(change => !string.IsNullOrWhiteSpace(change.Value));
            var project = this.ObservableForProperty(x => x.CurrentProject)
                              .Select(change => change.Value != null);



            return Extensions.BooleanAnd(username,password,homeDB,project);
        }
    }
}
