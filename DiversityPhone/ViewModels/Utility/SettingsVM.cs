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
using Funq;

namespace DiversityPhone.ViewModels.Utility
{
    public partial class SettingsVM : PageViewModel
    {
        Container IOC;
        ISettingsService Settings;
        
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
        private ISubject<bool> _CanResetSubject = new Subject<bool>();
        public ReactiveCommand ManageTaxa { get; private set; }

        private ReactiveAsyncCommand clearDatabase = new ReactiveAsyncCommand();
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelSubject = new Subject<AppSettings>();       
        
        public SettingsVM(Container ioc)            
        {
            IOC = ioc;
            Settings = ioc.Resolve<ISettingsService>();

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

            Reset = new ReactiveCommand(_CanResetSubject.StartWith(true));
            Messenger.RegisterMessageSource(
                Reset                
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

            Observable.Concat(
                Observable.Return(false),
                _IsFirstSetup.Select(x => !x),
                Observable.Never<bool>()
            )
            .Delay(TimeSpan.FromMilliseconds(500))
            .ObserveOnDispatcher() // Work around bug in ReactiveUI
            .Subscribe(_CanResetSubject);
             
            Save = new ReactiveCommand(_CanSaveSubject);

            RefreshVocabulary = new ReactiveAsyncCommand(_IsFirstSetup.Select(x => !x).StartWith(false));
            RefreshVocabulary
                .RegisterAsyncAction(_ => refreshVocabularyImpl(Settings.getSettings()));

            

            _IsBusy =
                RefreshVocabulary
                .ItemsInflight
                .Select(items => items > 0)
                .ToProperty(this, x => x.IsBusy);

            _BusyMessage = _BusyMessageSubject
                    .ToProperty(this, x => x.BusyMessage);

            _IsFirstSetup
                .Where(setup => setup)
                .Subscribe(_ => clearDatabase.Execute(null));

            _Setup = _IsFirstSetup 
                .Where(setup => setup)
                .Take(1)
                .Select(setup => new SetupVM(IOC.Resolve<IDiversityServiceClient>()))                        
                .ToProperty(this, x => x.Setup);
            _Setup
                .CombineLatest(_IsFirstSetup, (setup, _) => setup)
                .Subscribe(setup => setup.CurrentPivot = SetupVM.Pivots.Login);

            _Setup
                .SelectMany(s => (s != null) ? s.Result : Observable.Never<AppSettings>())                
                .Subscribe(cfg => _ModelSubject.OnNext(cfg));


            clearDatabase.RegisterAsyncAction(_ =>
                {
                    var taxa = IOC.Resolve<ITaxonService>();
                    var vocabulary = IOC.Resolve<IVocabularyService>();
                    var storage = IOC.Resolve<IFieldDataService>();

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
            
                                   
            var onsave =
            Save            
            .Do(_ => saveModel())
            .Publish();

            onsave
            .Where(_ => IsFirstSetup)
            .Do(_ => _IsFirstSetup.OnNext(false))
            .Do(_ => RefreshVocabulary //After Voc download, navigate to Taxon Page
                .AsyncCompletedNotification
                .Take(1)                
                .Subscribe(__ => ManageTaxa.Execute(null))
                )
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

            var storedConfig = Observable.Return(Settings.getSettings()).Concat(Observable.Never<AppSettings>());
            storedConfig.Subscribe(_ModelSubject);
            storedConfig.Select(x => x == null)
                .Subscribe(_IsFirstSetup);            
        }

        private void saveModel()
        {
            Model.UseGPS = UseGPS;
            Settings.saveSettings(Model);
            if (UseGPS == true)
            {
                if (App.Watcher==null)
                    App.startWatcher();
            }
            else
                if(App.Watcher!=null)
                    App.stopWatcher();
        }        

        
        private void OnReset()
        {
            Settings.saveSettings(null);            
            _IsFirstSetup.OnNext(true);
        }

        private void refreshVocabularyImpl(AppSettings settings)
        {            
            var credentials = new Svc.UserCredentials(settings);

            var vocabulary = IOC.Resolve<IVocabularyService>();
            var diversityService = IOC.Resolve<IDiversityServiceClient>();

            if (vocabulary == null || diversityService == null)
            {
#if DEBUG
                throw new ArgumentNullException("services");
#else
                return;
#endif
            }

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
