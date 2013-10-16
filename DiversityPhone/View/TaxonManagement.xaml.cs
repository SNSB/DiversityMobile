using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View {
    public partial class TaxonManagement : PhoneApplicationPage {
        private TaxonManagementVM VM { get { return DataContext as TaxonManagementVM; } }

        private CommandButtonAdapter _downloadall;
        public TaxonManagement() {
            InitializeComponent();

            if (VM != null) {
                var button = ApplicationBar.Buttons[0] as IApplicationBarIconButton;
                ApplicationBar.Buttons.Clear();
                _downloadall = new CommandButtonAdapter(
                    appbar: ApplicationBar,
                    button: button,
                    hideMode: CommandButtonAdapter.Mode.HideButton,
                    command: VM.DownloadAll);

            }
        }
    }
}