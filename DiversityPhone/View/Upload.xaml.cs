using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DiversityPhone.ViewModels.Utility;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class Upload : PhoneApplicationPage
    {
        private UploadVM VM { get { return DataContext as UploadVM; } }
        
        CommandButtonAdapter uploadall;

        public Upload()
        {
            InitializeComponent();
            this.BackKeyPress += Sync_BackKeyPress;
        }

        void Sync_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
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
        }        

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (uploadall == null && VM != null)
            {                
                uploadall = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.UploadAll);
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