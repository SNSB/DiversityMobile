using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;

namespace DiversityPhone.View.Setup {
    public partial class Project : PhoneApplicationPage {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        private OKNextPageCommandButton _OK;

        public Project() {
            InitializeComponent();

            _OK = new OKNextPageCommandButton(VM.GetProfile);

            this.ApplicationBar.Buttons.Add(
                _OK.Button
                );

        }
    }
}