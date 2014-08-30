namespace DiversityPhone.View
{
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using System.Windows.Controls;

    public partial class EditCS : PhoneApplicationPage
    {
        public EditCSVM VM { get { return DataContext as EditCSVM; } }

        private EditPageSaveEditButton _SaveEditBtn;
        private EditPageDeleteButton _delete;

        private EventBindingTrigger<TextBox> _AccessionNumberBinding;

        public EditCS()
        {
            InitializeComponent();

            _SaveEditBtn = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _AccessionNumberBinding = EventBindingTrigger<TextBox>.Create(AN_TB);
            DPControlBackGround.setTBBackgroundColor(AN_TB);
        }

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            DPControlBackGround.setTBBackgroundColor(AN_TB);
        }
    }
}