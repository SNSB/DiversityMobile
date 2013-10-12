namespace DiversityPhone.View {
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using System.Windows.Controls;

    public partial class EditIU : PhoneApplicationPage {
        private EditIUVM VM { get { return DataContext as EditIUVM; } }

        private EditPageSaveEditButton _appb;
        private EditPageDeleteButton _delete;
        private EventBindingTrigger<TextBox> _querystring;


        public EditIU() {
            InitializeComponent();

            _appb = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _querystring = EventBindingTrigger<TextBox>.Create(QueryString);
        }
    }
}