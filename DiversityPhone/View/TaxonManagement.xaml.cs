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
using DiversityPhone.ViewModels;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class TaxonManagement : PhoneApplicationPage
    {
        private TaxonManagementVM VM { get { return DataContext as TaxonManagementVM; } }

        private ProgressBinding<TaxonManagementVM> _progress;
        public TaxonManagement()
        {
            InitializeComponent();

            if (VM != null)
            {
                _progress = new ProgressBinding<TaxonManagementVM>(VM, x => x.IsBusy);


                var downloadAllButton = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.download.rest.png", UriKind.Relative),
                    IsEnabled = true,
                    Text = DiversityResources.TaxonManagement_Title_DownloadAll,
                };

                downloadAllButton.Click += new EventHandler(downloadAllButton_Click);

                ApplicationBar.Buttons.Add(downloadAllButton);

                VM.DownloadAll
                    .CanExecuteObservable
                    .Subscribe(canexec => downloadAllButton.IsEnabled = canexec);
            }


        }

        void downloadAllButton_Click(object sender, EventArgs e)
        {
            if (VM != null && VM.DownloadAll.CanExecute(null))
                VM.DownloadAll.Execute(null);
        }


        private void taxonPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (VM != null)
            //    e.Cancel = VM.IsBusy;
        }



        
    }
}