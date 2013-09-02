using DiversityPhone.View.Appbar;
using DiversityPhone.View.Helper;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

namespace DiversityPhone.View
{
    public partial class EditCS : PhoneApplicationPage
    {
        public EditCSVM VM { get{return DataContext as EditCSVM;} }

        private EditPageSaveEditButton _SaveEditBtn;        
        private EditPageDeleteButton _delete;

        private INPCBindingTrigger _AccessionNumberBinding;

        public EditCS()
        {
            InitializeComponent();

            _SaveEditBtn = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _AccessionNumberBinding = new INPCBindingTrigger(AN_TB);
            DPControlBackGround.setTBBackgroundColor(AN_TB);        
        }

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {            
            DPControlBackGround.setTBBackgroundColor(AN_TB);
        }      
    }
}