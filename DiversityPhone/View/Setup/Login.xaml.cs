using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

namespace DiversityPhone.View.Setup
{
    public partial class Login : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        private OKNextPageCommandButton _OK;

        private EventBindingTrigger<TextBox> _User;
        private EventBindingTrigger<PasswordBox> _Pass;

        public Login()
        {
            InitializeComponent();

            _User = EventBindingTrigger<TextBox>.Create(this.Username);

            _Pass = EventBindingTrigger<PasswordBox>.Create(this.Password);

            _OK = new OKNextPageCommandButton(VM.GetRepositories);

            this.ApplicationBar.Buttons.Add(_OK.Button
                );
        }
    }
}