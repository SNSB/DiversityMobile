using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows;

namespace DiversityPhone.View
{
    public partial class Upload : PhoneApplicationPage
    {
        private UploadVM VM { get { return DataContext as UploadVM; } }

        private CommandButtonAdapter uploadall;

        public Upload()
        {
            InitializeComponent();
            this.BackKeyPress += Sync_BackKeyPress;
        }

        private void Sync_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = VM;
            if (vm != null && vm.IsUploading)
            {
                if (vm.CancelUpload.CanExecute(null))
                {
                    vm.CancelUpload.Execute(null);
                    e.Cancel = true;
                }
            }
            else if (MMSelector.IsSelecting || FDSelector.IsSelecting)
            {
                FDSelector.IsSelecting = MMSelector.IsSelecting = false;
                e.Cancel = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (uploadall == null && VM != null)
            {
                uploadall = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.StartUpload);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (uploadall != null)
            {
                uploadall.Dispose();
                uploadall = null;
            }
        }
    }
}