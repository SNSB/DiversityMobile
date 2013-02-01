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
using DiversityPhone.View.Helper;

namespace DiversityPhone.View
{
    public partial class EditAnalysis : PhoneApplicationPage
    {
        private EditAnalysisVM VM { get { return DataContext as EditAnalysisVM; } }

        private EditPageSaveEditButton _appb;
        private INPCBindingTrigger _customresult;

        public EditAnalysis()
        {
            InitializeComponent();            
            DPControlBackGround.setTBBackgroundColor(TB_CustomResult);      
            _appb = new EditPageSaveEditButton(ApplicationBar, VM);
            _customresult = new INPCBindingTrigger(TB_CustomResult);
        }

        private void TB_CustomResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            DPControlBackGround.setTBBackgroundColor(TB_CustomResult);
        }
    }
}