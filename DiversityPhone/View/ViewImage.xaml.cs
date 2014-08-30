using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;

namespace DiversityPhone.View
{
    public partial class ImagePage : PhoneApplicationPage
    {
        private ViewImageVM VM
        {
            get
            {
                return DataContext as ViewImageVM;
            }
        }

        public ImagePage()
        {
            this.InitializeComponent();

            new EditPageDeleteButton(this.ApplicationBar, VM);
        }
    }
}