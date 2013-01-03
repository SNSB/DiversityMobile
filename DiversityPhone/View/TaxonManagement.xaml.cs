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
using DiversityPhone.View.Appbar;

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