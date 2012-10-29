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
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class EditAnalysis : PhoneApplicationPage
    {
        private EditAnalysisVM VM { get { return DataContext as EditAnalysisVM; } }

        private EditPageSaveEditButton _appb;
        private IList<Control> _toStore;

        public EditAnalysis()
        {
            InitializeComponent();
            _toStore = new List<Control> { TB_CustomResult};
            DPControlBackGround.adjustStoreBackgroundColors(_toStore);      
            _appb = new EditPageSaveEditButton(ApplicationBar, VM);
        }

        private void TB_CustomResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            DPControlBackGround.setTBBackgroundColor(TB_CustomResult);
        }
    }
}