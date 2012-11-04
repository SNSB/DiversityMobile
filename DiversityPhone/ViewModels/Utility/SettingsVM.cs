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
    public partial class SettingsVM : PageVMBase
    {
        
        ISettingsService Settings;        
        NavigationService Navigation;

        

        

        public ReactiveCommand RefreshVocabulary{get; private set;}      


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
        
        public ReactiveCommand ManageTaxa { get; private set; }

        public ReactiveCommand UploadData { get; private set; }       
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        private ISubject<AppSettings> _ModelSubject = new Subject<AppSettings>();       
        
        public SettingsVM(Container ioc)            
        {            
            Settings = ioc.Resolve<ISettingsService>();
            Navigation = ioc.Resolve<NavigationService>();

            _Model = this.ObservableToProperty(
                _ModelSubject   
                .Where(x => true),
                x => x.Model);            

            _ModelSubject
                .Where(x => x != null)
                .Select(m => m.UseGPS)
                .Subscribe(x => UseGPS = x);                      

            Reset = new ReactiveCommand();
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
             
            Save = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                Save
                .Do(_ => saveModel())
                .Select(_ => Page.Previous)
                );

            RefreshVocabulary = new ReactiveCommand();
            RefreshVocabulary
                .Subscribe(_ => 
                {
                    Messenger.SendMessage(Page.Setup);
                });           
            
                                   
            
               

            ManageTaxa = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ManageTaxa
                .Select(_ => Services.Page.TaxonManagement)
                );

            UploadData = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                UploadData
                .Select(_ => Services.Page.Sync)
                );

            var storedConfig = Observable.Return(Settings.getSettings()).Concat(Observable.Never<AppSettings>());
            storedConfig.Subscribe(_ModelSubject);                       
        }

       

        private void saveModel()
        {
            Model.UseGPS = UseGPS;
            Settings.saveSettings(Model);           
        }        

        
        private void OnReset()
        {
            Settings.saveSettings(null);
            Messenger.SendMessage(Page.Setup);
        }     
    }
}
