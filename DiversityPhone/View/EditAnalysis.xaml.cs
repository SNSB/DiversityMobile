using DiversityPhone.View.Appbar;
using DiversityPhone.View.Helper;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

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