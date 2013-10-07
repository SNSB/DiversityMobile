using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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
                _downloadall = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.DownloadAll);

            }
        } 
    }
}