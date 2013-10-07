using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;

namespace DiversityPhone.View.Setup
{
    public partial class Project : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        public Project()
        {
            InitializeComponent();

            this.ApplicationBar.Buttons.Add(
                new OKNextPageButton(VM.Messenger, Model.Page.SetupGPS, VM.Profile.IsProfileValid)
                );

        }
    }
}