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
using System.Reactive.Disposables;

namespace DiversityPhone.ViewModels.Utility
{
    public partial class SettingsVM : PageViewModel
    {
        ISettingsService _settings;
        IDiversityServiceClient _DivSvc;
        IOfflineStorage _storage;


        public SetupVM Setup { get { return _Setup.Value; } }
        private ObservableAsPropertyHelper<SetupVM> _Setup;
        private SerialDisposable _SetupSubscription = new SerialDisposable();
        

        public bool IsFirstSetup { get { return _IsFirstSetup.Value; } }
        private ObservableAsPropertyHelper<bool> _IsFirstSetup;

        public bool IsBusy { get { return _IsBusy.Value; } }        
        private ISubject<bool> _IsBusySubject = new Subject<bool>();
        private ObservableAsPropertyHelper<bool> _IsBusy;       


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
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelSubject = new Subject<AppSettings>();       
        
        public SettingsVM(ISettingsService set, IDiversityServiceClient divsvc, IOfflineStorage storage)            
        {
            _settings = set;          
            _DivSvc = divsvc;
            _storage = storage;     

            _Model =_ModelSubject                
                .ToProperty(this, x => x.Model);            

            _ModelSubject
                .Select(m => m.UseGPS)
                .BindTo(this, x => x.UseGPS);

            _IsFirstSetup =
                _ModelSubject
                .Select( x => x.UserName == null)                
                .ToProperty(this, x => x.IsFirstSetup);

            Reset = new ReactiveCommand(_IsFirstSetup.Select(x => !x));
            Save = new ReactiveCommand(_CanSaveSubject);

            _Setup = _IsFirstSetup
                .Where(x => x)
                .Select(_ => new SetupVM(this))
                .Do(setup => _SetupSubscription.Disposable = setup.CanSave.Subscribe(_CanSaveSubject))
                .ToProperty(this, x => x.Setup);
                
                

            _IsFirstSetup
                .Where(x => !x)                
                .Subscribe(_ => OnSettings());

            _IsBusy = _IsBusySubject
                .ToProperty(this, x => x.IsBusy);

            

            
                                   
            Messenger.RegisterMessageSource(
                Save
                .Where(_ => !IsFirstSetup)
                .Do(_ => saveModel())
                .Select(_ => new NavigationMessage(Services.Page.Previous))
                );

            ManageTaxa = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ManageTaxa
                .Select(_ => new NavigationMessage(Services.Page.TaxonManagement))
                );


            _ModelSubject.OnNext(_settings.getSettings());            
        }

        private void saveModel()
        {
            Model.UseGPS = UseGPS;

            _settings.saveSettings(Model);
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
            _ModelSubject.OnNext(new AppSettings());
        }

       

        
    }
}
