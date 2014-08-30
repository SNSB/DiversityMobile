namespace DiversityPhone.View
{
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using System.Windows.Controls;

    public partial class EditEV : PhoneApplicationPage
    {
        private EditEVVM VM { get { return DataContext as EditEVVM; } }

        private EditPageSaveEditButton _appbar;
        private EditPageDeleteButton _delete;
        private EventBindingTrigger<TextBox> _LocalityBinding, _HabitatBinding;

        public EditEV()
        {
            InitializeComponent();

            _appbar = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);
            _LocalityBinding = EventBindingTrigger<TextBox>.Create(LocalityTB);
            _HabitatBinding = EventBindingTrigger<TextBox>.Create(HabitatTB);

            DPControlBackGround.setTBBackgroundColor(LocalityTB);
        }

        private void LocalityTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            DPControlBackGround.setTBBackgroundColor(LocalityTB);
        }
    }
}