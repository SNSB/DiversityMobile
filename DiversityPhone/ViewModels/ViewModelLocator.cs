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
using Funq;
using DiversityPhone.Services;
using ReactiveUI;
using DiversityPhone.DiversityService;
using DiversityPhone.ViewModels.Utility;
using DiversityPhone.ViewModels.Maps;

namespace DiversityPhone.ViewModels
{
    public partial class ViewModelLocator
    {
        private const string OFFLINE_STORAGE = "OfflineStorage";
        private static Container IOC { get { return App.IOC; } }   

        private static HomeVM _homeVM;        

        public ViewModelLocator()
        {
        }

        public SyncVM Sync { get { return IOC.Resolve<SyncVM>(); } }

        public HomeVM Home { get { return IOC.Resolve<HomeVM>(); } }

        public EditESVM EditES { get { return IOC.Resolve<EditESVM>(); } }
        public ViewESVM ViewES { get { return IOC.Resolve<ViewESVM>(); } }

        public EditEVVM EditEV { get { return IOC.Resolve<EditEVVM>(); } }
        public ViewEVVM ViewEV { get { return IOC.Resolve<ViewEVVM>(); } }

        public EditCSVM EditCS { get { return IOC.Resolve<EditCSVM>(); } }
        public ViewCSVM ViewCS { get { return IOC.Resolve<ViewCSVM>(); } }

        public EditIUVM EditIU { get { return IOC.Resolve<EditIUVM>(); } }
        public ViewIUVM ViewIU { get { return IOC.Resolve<ViewIUVM>(); } }
        public EditAnalysisVM EditAnalysis { get { return IOC.Resolve<EditAnalysisVM>(); } }
        public EditPropertyVM EditProperty { get { return IOC.Resolve<EditPropertyVM>(); } }


        public ViewMapPickerVM ViewLM { get { return IOC.Resolve<ViewMapPickerVM>(); } }
        public ViewDLMVM ViewDLM{ get { return IOC.Resolve<ViewDLMVM>(); } }
        public ViewDownloadMapsVM ViewDownloadMaps { get { return IOC.Resolve<ViewDownloadMapsVM>(); } }
        public EditMapVM EditMap { get { return IOC.Resolve<EditMapVM>(); } }
        public ViewMapVM ViewMap { get { return IOC.Resolve<ViewMapVM>(); } }
        public ViewMapESVM ViewMapES { get { return IOC.Resolve<ViewMapESVM>(); } }
        public ViewMapEVVM ViewMapEV { get { return IOC.Resolve<ViewMapEVVM>(); } }
        public ViewMapIUVM ViewMapIU { get { return IOC.Resolve<ViewMapIUVM>(); } }

        public SelectNewMMOVM SelectNewMMO { get { return IOC.Resolve <SelectNewMMOVM>(); } }
        public NewImageVM NewImage { get { return IOC.Resolve<NewImageVM>(); } }
        public NewAudioVM NewAudio { get { return IOC.Resolve<NewAudioVM>(); } }
        public NewVideoVM NewVideo { get { return IOC.Resolve<NewVideoVM>(); } }
        public ViewImageVM ViewImage { get { return IOC.Resolve<ViewImageVM>(); } }
        public ViewAudioVM ViewAudio { get { return IOC.Resolve<ViewAudioVM>(); } }
        public ViewVideoVM ViewVideo { get { return IOC.Resolve<ViewVideoVM>(); } }

        public TaxonManagementVM TaxonManagement { get { return IOC.Resolve<TaxonManagementVM>(); } }
        public SettingsVM Settings { get { return IOC.Resolve<SettingsVM>(); } }

        public SetupVM Setup { get { return IOC.Resolve<SetupVM>(); } }
       
    }
}
