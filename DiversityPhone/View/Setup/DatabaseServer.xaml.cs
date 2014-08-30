namespace DiversityPhone.View.Setup
{
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;

    public partial class DatabaseServer : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        private OKNextPageCommandButton _OK;

        public DatabaseServer()
        {
            InitializeComponent();

            _OK = new OKNextPageCommandButton(VM.GetProjects);

            this.ApplicationBar.Buttons.Add(
                _OK.Button
                );
        }
    }
}