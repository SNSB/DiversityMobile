using DiversityPhone.Model;
using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using ReactiveUI;

namespace DiversityPhone.View.Setup
{
    public partial class Login : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        public Login()
        {
            InitializeComponent();

            this.ApplicationBar.Buttons.Add(
                new OKNextPageButton(VM.Messenger, Page.SetupDatabase, VM.Login.IsLoginValid)
                );
        }
    }
}