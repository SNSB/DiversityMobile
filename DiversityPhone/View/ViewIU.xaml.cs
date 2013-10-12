
using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class ViewIU : PhoneApplicationPage
    {
        private CommandButtonAdapter _add, _maps;

        NewMultimediaAppBarUpdater _mmo;

        private ViewIUVM VM { get { return DataContext as ViewIUVM; } }

        public ViewIU()
        {
            InitializeComponent();

            _add = new CommandButtonAdapter(ApplicationBar.Buttons[0] as ApplicationBarIconButton, VM.Add);
            _maps = new CommandButtonAdapter(ApplicationBar.Buttons[1] as ApplicationBarIconButton, VM.Maps);
            _mmo = new NewMultimediaAppBarUpdater(VM.Messenger, this, VM.MultimediaList);
        }
       
        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (VM != null)
            {
                VM.Back.Execute(null);
                e.Cancel = true;
            }
        }
    }
}