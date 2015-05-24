using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Linq;

namespace DiversityPhone.View
{
    public partial class TaxonManagement : PhoneApplicationPage
    {
        private TaxonManagementVM VM { get { return DataContext as TaxonManagementVM; } }

        private CommandButtonAdapter _downloadall;

        public TaxonManagement()
        {
            InitializeComponent();

            if (VM != null)
            {
                var button = ApplicationBar.Buttons[0] as IApplicationBarIconButton;
                ApplicationBar.Buttons.Clear();
                _downloadall = new CommandButtonAdapter(
                    appbar: ApplicationBar,
                    button: button,
                    hideMode: CommandButtonAdapter.Mode.HideButton,
                    command: VM.DownloadAll);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (VM != null)
            {
                var inProgress = from list in VM.LocalLists
                                 where list.IsDownloading
                                 select list;

                if (inProgress.Any())
                {
                    e.Cancel = true;

                    VM.Notifications.showPopup(DiversityResources.TaxonManagement_Info_DownloadInProgress);
                }
            }

            base.OnBackKeyPress(e);
        }
    }
}