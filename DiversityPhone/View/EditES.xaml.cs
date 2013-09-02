using DiversityPhone.View.Appbar;
using DiversityPhone.View.Helper;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;

namespace DiversityPhone.View
{
    public partial class EditES : PhoneApplicationPage
    {
        private EditESVM VM { get { return this.DataContext as EditESVM; } }

        private EditPageSaveEditButton _appbarupd;
        private EditPageDeleteButton _delete;

        private INPCBindingTrigger _DescriptionBinding, _CodeBinding;
       
        public EditES()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-De");//Zum setzen eines Default gut genug. Über UserProfile Customizable machen.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-De");
            InitializeComponent();

            _appbarupd = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _DescriptionBinding = new INPCBindingTrigger(DescTB);
            _CodeBinding = new INPCBindingTrigger(SeriesCodeTB);

            DPControlBackGround.setTBBackgroundColor(DescTB);         
        }       

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {            
            DPControlBackGround.setTBBackgroundColor(DescTB);
        }

       
    }
}