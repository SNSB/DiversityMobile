using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;

namespace DiversityPhone.View.Setup
{
    public partial class DatabaseServer : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        public DatabaseServer()
        {
            InitializeComponent();

            this.ApplicationBar.Buttons.Add(
                new OKNextPageButton(VM.Messenger, Model.Page.SetupProject, VM.Database.IsDatabaseSelected)
                );

        }
    }
}