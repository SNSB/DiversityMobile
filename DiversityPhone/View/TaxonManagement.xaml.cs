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
                _progress = new ProgressBinding<TaxonManagementVM>(VM, x => x.IsBusy);
        }

        private void taxonPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (VM != null)
                e.Cancel = VM.IsBusy;
        }

        
    }
}