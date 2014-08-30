namespace DiversityPhone.View
{
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Controls;

    public partial class EditES : PhoneApplicationPage
    {
        private EditESVM VM { get { return this.DataContext as EditESVM; } }

        private EditPageSaveEditButton _appbarupd;
        private EditPageDeleteButton _delete;

        private EventBindingTrigger<TextBox> _DescriptionBinding, _CodeBinding;

        public EditES()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-De");//Zum setzen eines Default gut genug. Über UserProfile Customizable machen.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-De");
            InitializeComponent();

            _appbarupd = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _DescriptionBinding = EventBindingTrigger<TextBox>.Create(DescTB);
            _CodeBinding = EventBindingTrigger<TextBox>.Create(SeriesCodeTB);

            DPControlBackGround.setTBBackgroundColor(DescTB);
        }

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            DPControlBackGround.setTBBackgroundColor(DescTB);
        }
    }
}