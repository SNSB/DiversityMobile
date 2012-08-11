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
    public partial class Sync : PhoneApplicationPage
    {
        private SyncVM VM { get { return DataContext as SyncVM; } }

        ProgressBinding<SyncVM> progress;
        CommandButtonAdapter uploadall;

        public Sync()
        {
            InitializeComponent();           
        }        

        private void syncPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (progress == null && VM != null)
            {
                progress = new ProgressBinding<SyncVM>(VM, x => x.IsBusy);
                uploadall = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.UploadAll);
            }
        }

        private void syncPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (progress != null)
            {
                progress.Dispose();
                progress = null;
            }
            if (uploadall != null)
            {
                uploadall.Dispose();
                uploadall = null;
            }
        }
    }
}