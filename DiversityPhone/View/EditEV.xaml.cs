using DiversityPhone.View.Appbar;
using DiversityPhone.View.Helper;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using System.Windows.Controls;



namespace DiversityPhone.View
{
    public partial class EditEV : PhoneApplicationPage
    {
        private EditEVVM VM { get { return DataContext as EditEVVM; } }
        private EditPageSaveEditButton _appbar;        
        private EditPageDeleteButton _delete;
        private INPCBindingTrigger _LocalityBinding, _HabitatBinding;
       

        public EditEV()
        {
            InitializeComponent();

            _appbar = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);
            _LocalityBinding = new INPCBindingTrigger(LocalityTB);
            _HabitatBinding = new INPCBindingTrigger(HabitatTB);

            DPControlBackGround.setTBBackgroundColor(LocalityTB);
        }      

        private void LocalityTB_TextChanged(object sender, TextChangedEventArgs e)
        {            
            DPControlBackGround.setTBBackgroundColor(LocalityTB);            
        }
    }
}