namespace DiversityPhone.View
{
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using System.Windows.Controls;

    public partial class EditAnalysis : PhoneApplicationPage
    {
        private EditAnalysisVM VM { get { return DataContext as EditAnalysisVM; } }

        private EditPageSaveEditButton _appb;
        private EventBindingTrigger<TextBox> _customresult;

        public EditAnalysis()
        {
            InitializeComponent();
            DPControlBackGround.setTBBackgroundColor(TB_CustomResult);
            _appb = new EditPageSaveEditButton(ApplicationBar, VM);
            _customresult = EventBindingTrigger<TextBox>.Create(TB_CustomResult);
        }

        private void TB_CustomResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            DPControlBackGround.setTBBackgroundColor(TB_CustomResult);
        }
    }
}