using System;
using DiversityPhone.Services;
using DiversityPhone.Model;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using ReactiveUI;
using System.Reactive.Subjects;

using System.Reactive;
using DiversityPhone.Interface;

namespace DiversityPhone.ViewModels.Utility
{
    public partial class SettingsVM : PageVMBase
    {
        readonly ISettingsService Settings;
        readonly ICleanupData Cleanup;
        readonly IConnectivityService Connectivity;





        public ReactiveCommand RefreshVocabulary { get; private set; }


        private bool _UseGPS;

        public bool UseGPS
        {
            get
            {
                return _UseGPS;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.UseGPS, ref _UseGPS, value);
            }
        }
        public ReactiveCommand Save { get; private set; }

        public ReactiveCommand Reset { get; private set; }

        public ReactiveCommand ManageTaxa { get; private set; }

        public ReactiveCommand UploadData { get; private set; }
        public ReactiveCommand DownloadData { get; private set; }

        public ReactiveCommand Info { get; private set; }



        private AppSettings _Model;

        public AppSettings Model
        {
            get
            {
                return _Model;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value);
            }
        }

        public SettingsVM(
            ISettingsService Settings,
            ICleanupData Cleanup,
            IConnectivityService Connectivity
            )
        {
            this.Cleanup = Cleanup;
            this.Settings = Settings;
            this.Connectivity = Connectivity;

            this.WhenAny(x => x.Model, x => x.Value)
                .Where(x => x != null)
                .Select(m => m.UseGPS)
                .Subscribe(x => UseGPS = x);

            Reset = new ReactiveCommand(Connectivity.WifiAvailable());
            Messenger.RegisterMessageSource(
                Reset
                .Select(_ => new DialogMessage(
                    DialogType.YesNo,
                    "Are you sure?",
                    "All diversity data you have not already uploaded will be lost!",
                    res =>
                    {
                        if (res == DialogResult.OKYes)
                            OnReset();
                    }
                    )));

            var setting_changed =
                this.WhenAny(x => x.UseGPS, x => x.Model, 
                    (gps, model) => (model.Value != null) ? model.Value.UseGPS != gps.Value : false);

            Save = new ReactiveCommand(setting_changed);
            Messenger.RegisterMessageSource(
                Save
                .Do(_ => saveModel())
                .Select(_ => Page.Previous)
                );

            RefreshVocabulary = new ReactiveCommand(Connectivity.WifiAvailable());
            RefreshVocabulary
                .Subscribe(_ =>
                {
                    Messenger.SendMessage(Page.SetupVocabulary);
                });





            ManageTaxa = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                ManageTaxa
                .Select(_ => Page.TaxonManagement)
                );

            UploadData = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                UploadData
                .Select(_ => Page.Upload)
                );

            DownloadData = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                DownloadData
                .Select(_ => Page.Download)
                );

            Info = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                Info
                .Select(_ => Page.Info)
                );

            Settings
                .SettingsObservable()
                .Subscribe(x => Model = x);            
        }



        private void saveModel()
        {
            Model.UseGPS = UseGPS;
            Settings.SaveSettings(Model);
        }


        private void OnReset()
        {
            Cleanup.ClearLocalData();
            Messenger.SendMessage(Page.SetupWelcome);
        }
    }
}