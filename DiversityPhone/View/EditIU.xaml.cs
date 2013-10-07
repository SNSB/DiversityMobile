using DiversityPhone.View.Appbar;
using DiversityPhone.View.Helper;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;

namespace DiversityPhone.View
{
    public partial class EditIU : PhoneApplicationPage
    {
        private EditIUVM VM { get { return DataContext as EditIUVM; } }

        private EditPageSaveEditButton _appb;
        private EditPageDeleteButton _delete;
        private INPCBindingTrigger _querystring;


        public EditIU()
        {
            InitializeComponent();

            _appb = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _querystring = new INPCBindingTrigger(QueryString);
        }        
    }
}