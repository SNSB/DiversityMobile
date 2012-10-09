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

namespace DiversityPhone.ViewModels
{
    public partial class ViewModelLocator
    {
        private const string OFFLINE_STORAGE = "OfflineStorage";
        private static Container IOC { get { return App.IOC; } }   

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

        public MapManagementVM MapManagement { get { return IOC.Resolve<MapManagementVM>(); } }               
        public ViewMapVM ViewMap { get { return IOC.Resolve<ViewMapVM>(); } }        
        
        public ImageVM Image { get { return IOC.Resolve<ImageVM>(); } }
        public AudioVM NewAudio { get { return IOC.Resolve<AudioVM>(); } }
        public VideoVM NewVideo { get { return IOC.Resolve<VideoVM>(); } }

        public TaxonManagementVM TaxonManagement { get { return IOC.Resolve<TaxonManagementVM>(); } }
        public SettingsVM Settings { get { return IOC.Resolve<SettingsVM>(); } }

        public SetupVM Setup { get { return IOC.Resolve<SetupVM>(); } }
       
    }
}
