using System;
using DiversityPhone.Services;
using DiversityPhone.Model;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using ReactiveUI;
using System.Linq;
using System.Collections.Generic;
using Svc = DiversityPhone.DiversityService;
using System.Reactive.Subjects;
using DiversityPhone.Messages;
using System.Reactive.Disposables;

namespace DiversityPhone.ViewModels.Utility
{
    public partial class SettingsVM : PageViewModel
    {
        ISettingsService _settings;
        IDiversityServiceClient _DivSvc;
        IVocabularyService _vocabulary;
        IFieldDataService _storage;

        public SetupVM Setup { get { return _Setup.Value; } }
        private ObservableAsPropertyHelper<SetupVM> _Setup;

        public bool IsFirstSetup { get { return _IsFirstSetupHelper.Value; } }
        private ObservableAsPropertyHelper<bool> _IsFirstSetupHelper;
        private ISubject<bool> _IsFirstSetup = new Subject<bool>();  

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        public string BusyMessage { get { return _BusyMessage.Value; } }
        private ISubject<string> _BusyMessageSubject = new Subject<string>();
        private ObservableAsPropertyHelper<string> _BusyMessage;

        public ReactiveAsyncCommand RefreshVocabulary{get; private set;}


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
        private ISubject<bool> _CanSaveSubject = new Subject<bool>();

        public ReactiveCommand Reset { get; private set; }

        

        public ReactiveCommand ManageTaxa { get; private set; }

        private ReactiveAsyncCommand clearDatabase = new ReactiveAsyncCommand();
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelSubject = new Subject<AppSettings>();       
        
        public SettingsVM(ISettingsService set, IDiversityServiceClient divsvc, IFieldDataService storage, IVocabularyService voc)            
        {
            _settings = set;          
            _DivSvc = divsvc;
            _storage = storage;
            _vocabulary = voc;

            _Model =_ModelSubject   
                .Where(x => true)
                .ToProperty(this, x => x.Model);            

            _ModelSubject
                .Where(x => x != null)
                .Select(m => m.UseGPS)
                .BindTo(this, x => x.UseGPS);

            _ModelSubject
                .Select(x => x != null)
                .StartWith(false)
                .Subscribe(_CanSaveSubject);

            _IsFirstSetupHelper =                
                _IsFirstSetup
                .ToProperty(this, x => x.IsFirstSetup);

            Reset = new ReactiveCommand(_IsFirstSetup.Select(x => !x).StartWith(false));
            Messenger.RegisterMessageSource(
                Reset
                .Take(1)
                .Select(_ => new DialogMessage(
                    Messages.DialogType.YesNo,
                    "Are you sure?",
                    "All diversity data you have not already uploaded will be lost!",
                    res =>
                    {
                        if (res == DialogResult.OKYes)
                            OnReset();
                    }
                    )));
             
            Save = new ReactiveCommand(_CanSaveSubject);

            RefreshVocabulary = new ReactiveAsyncCommand(_IsFirstSetup.Select(x => !x).StartWith(false));
            RefreshVocabulary
                .RegisterAsyncAction(_ => refreshVocabularyImpl(_settings.getSettings()));

            _IsBusy =
                RefreshVocabulary
                .ItemsInflight
                .Select(items => items > 0)
                .ToProperty(this, x => x.IsBusy);

            _BusyMessage = _BusyMessageSubject
                    .ToProperty(this, x => x.BusyMessage);

            _Setup = _IsFirstSetup
                .Where(x => x)
                .Select(_ => new SetupVM(_DivSvc))                                
                .Do(_ => clearDatabase.Execute(null))                
                .ToProperty(this, x => x.Setup);
            _Setup
                .SelectMany(s => s.Result)
                .ObserveOnDispatcher()
                .Subscribe(cfg => _ModelSubject.OnNext(cfg));


            clearDatabase.RegisterAsyncAction(_ => _storage.clearDatabase());
            
                                   
            var onsave =
            Save            
            .Do(_ => saveModel())
            .Publish();

            onsave
            .Where(_ => IsFirstSetup)
            .Do(_ => _IsFirstSetup.OnNext(false))
            .Subscribe(RefreshVocabulary.Execute);

            Messenger.RegisterMessageSource(
                onsave
                .Where(_ => !IsFirstSetup)
                .Select(_ => Page.Previous));

            onsave.Connect();
               

            ManageTaxa = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ManageTaxa
                .Select(_ => new NavigationMessage(Services.Page.TaxonManagement))
                );

            var storedConfig = Observable.Return(_settings.getSettings()).Concat(Observable.Never<AppSettings>());
            storedConfig.Subscribe(_ModelSubject);
            storedConfig.Select(x => x == null)
                .Subscribe(_IsFirstSetup);            
        }

        private void saveModel()
        {
            Model.UseGPS = UseGPS;
            _settings.saveSettings(Model);
            if (UseGPS == true)
            {
                if (App.Watcher.Status != System.Device.Location.GeoPositionStatus.Ready)
                    App.startWatcher();
            }
            else
                if(App.Watcher.Status==System.Device.Location.GeoPositionStatus.Ready || App.Watcher.Status==System.Device.Location.GeoPositionStatus.Initializing)
                    App.stopWatcher();
        }        

        
        private void OnReset()
        {            
            _settings.saveSettings(null);
            _ModelSubject.OnNext(null);
            _IsFirstSetup.OnNext(true);
        }

        private void refreshVocabularyImpl(AppSettings settings)
        {            
            var credentials = new Svc.UserCredentials(settings);

            var vocabulary = _vocabulary;
            var diversityService = _DivSvc;

            vocabulary.clearVocabulary();

            _BusyMessageSubject.OnNext("Downloading Vocabulary");
            var voc = diversityService.GetStandardVocabulary().First();
            var analysesObservable = diversityService.GetAnalysesForProject(settings.CurrentProject, credentials);
            vocabulary.addTerms(voc);

            _BusyMessageSubject.OnNext("Downloading Analyses");
            var analyses = analysesObservable.First();
            var resultObservable = diversityService.GetAnalysisResultsForProject(settings.CurrentProject, credentials);

            vocabulary.addAnalyses(analyses);

            _BusyMessageSubject.OnNext("Downloading Analysis Results");

            var results = resultObservable.First();
            var atgObservable = diversityService.GetAnalysisTaxonomicGroupsForProject(settings.CurrentProject, credentials);

            vocabulary.addAnalysisResults(results);

            var atgs = atgObservable.First();

            vocabulary.addAnalysisTaxonomicGroups(atgs);           
        }

        
    }
}
