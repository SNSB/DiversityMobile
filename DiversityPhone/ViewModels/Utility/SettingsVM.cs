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
using DiversityPhone.Services.BackgroundTasks;
using DiversityPhone.DiversityService;

namespace DiversityPhone.ViewModels.Utility
{
    public partial class SettingsVM : PageViewModel
    {
        
        ISettingsService Settings;
        IGeoLocationService GeoLocation;
        
        public SetupVM Setup { get { return _Setup.Value; } }
        private ObservableAsPropertyHelper<SetupVM> _Setup;

        public bool IsFirstSetup { get { return _IsFirstSetupHelper.Value; } }
        private ObservableAsPropertyHelper<bool> _IsFirstSetupHelper;
        private ISubject<bool> _IsFirstSetup = new Subject<bool>();  

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        public string BusyMessage { get { return _BusyMessage.Value; } }
        private ObservableAsPropertyHelper<string> _BusyMessage;

        public ReactiveCommand RefreshVocabulary{get; private set;}
        public ReactiveCommand NavigateBack { get; private set; }


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

        public ReactiveCommand UploadData { get; private set; }

        private ReactiveAsyncCommand clearDatabase = new ReactiveAsyncCommand();
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelSubject = new Subject<AppSettings>();       
        
        public SettingsVM(Container ioc)            
        {            
            Settings = ioc.Resolve<ISettingsService>();
            GeoLocation = ioc.Resolve<IGeoLocationService>();
            var background = ioc.Resolve<IBackgroundService>();
            var refreshVocabularyTask = background.getTaskObject<RefreshVocabularyTask>();
            

            _Model = this.ObservableToProperty(
                _ModelSubject   
                .Where(x => true),
                x => x.Model);            

            _ModelSubject
                .Where(x => x != null)
                .Select(m => m.UseGPS)
                .BindTo(this, x => x.UseGPS);

            _ModelSubject
                .Select(x => x != null)
                .StartWith(false)
                .Subscribe(_CanSaveSubject);

            _IsFirstSetupHelper = this.ObservableToProperty(               
                _IsFirstSetup, x => x.IsFirstSetup);

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

            RefreshVocabulary = new ReactiveCommand(
                _IsFirstSetup.Select(x => !x)
                .StartWith(IsFirstSetup)
                .CombineLatest(
                    refreshVocabularyTask.BusyObservable
                    .StartWith(refreshVocabularyTask.IsBusy),
                    (notfirstsetup, busy) => notfirstsetup && !busy)
                );
            RefreshVocabulary
                .Subscribe(_ => background.startTask<RefreshVocabularyTask>(new UserCredentials(Settings.getSettings())));

            NavigateBack = new ReactiveCommand();
            NavigateBack
                .Subscribe(_ =>
                {
                    if (IsFirstSetup || IsBusy)
                        Messenger.SendMessage<DialogMessage>(
                            new DialogMessage(DialogType.OK,
                                DiversityResources.Message_SorryHeader,
                                DiversityResources.Setup_Message_CantGoBack_Body));
                    else
                        Messenger.SendMessage(Page.Previous);
                });
                

            _IsBusy = this.ObservableToProperty(
                refreshVocabularyTask
                .BusyObservable, x => x.IsBusy);

            _BusyMessage = this.ObservableToProperty(
                refreshVocabularyTask.AsyncProgressMessages, x => x.BusyMessage);

            _IsFirstSetup
                .Where(setup => setup)
                .Subscribe(_ => clearDatabase.Execute(null));

            _Setup = this.ObservableToProperty( 
                _IsFirstSetup 
                .Where(setup => setup)
                .Take(1)
                .Select(setup => new SetupVM(ioc)),
                x => x.Setup);
            _Setup
                .CombineLatest(_IsFirstSetup, (setup, _) => setup)
                .Subscribe(setup => setup.CurrentPivot = SetupVM.Pivots.Login);

            _Setup
                .SelectMany(s => (s != null) ? s.Result : Observable.Never<AppSettings>())                
                .Subscribe(cfg => _ModelSubject.OnNext(cfg));


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
            
                                   
            var onsave =
            Save            
            .Do(_ => saveModel())
            .Publish();

            onsave
            .Where(_ => IsFirstSetup)
            .Do(_ => _IsFirstSetup.OnNext(false))
            .Do(_ => refreshVocabularyTask //After Voc download, navigate to Taxon Page
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

            UploadData = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                UploadData
                .Select(_ => new NavigationMessage(Services.Page.Sync))
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
                if (GeoLocation.IsWatching() == false)
                    GeoLocation.startWatcher();
            }
            else
                if (GeoLocation.IsWatching() == true)
                    GeoLocation.stopWatcher();
        }        

        
        private void OnReset()
        {
            Settings.saveSettings(null);            
            _IsFirstSetup.OnNext(true);
        }     
    }
}
