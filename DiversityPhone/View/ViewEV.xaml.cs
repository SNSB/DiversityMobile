
namespace DiversityPhone {
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using System;

    public partial class ViewEV : PhoneApplicationPage {
        private NewMultimediaAppBarUpdater _mmo_appbar;
        private CommandButtonAdapter _add;

        private ViewEVVM VM {
            get {
                return DataContext as ViewEVVM;
            }
        }

        public ViewEV() {
            InitializeComponent();

            _mmo_appbar = new NewMultimediaAppBarUpdater(VM.Messenger, this, VM.MultimediaList);
            _add = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.Add);
        }

        private void Map_Click(object sender, EventArgs e) {
            if (VM != null)
                VM.Maps.Execute(null);
        }
    }
}