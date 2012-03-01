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
using DiversityPhone.Model;
using DiversityPhone.ViewModels;

namespace DiversityPhone.View
{
    public partial class EditAnalysis : PhoneApplicationPage
    {
        private EditAnalysisVM VM { get { return DataContext as EditAnalysisVM; } }

        private EditPageAppBarUpdater<IdentificationUnitAnalysis> _appb;

        public EditAnalysis()
        {
            InitializeComponent();

            _appb = new EditPageAppBarUpdater<IdentificationUnitAnalysis>(ApplicationBar, VM);
        }
    }
}