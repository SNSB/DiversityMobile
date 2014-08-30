using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class NewAudio : PhoneApplicationPage
    {
        private AudioVM VM
        {
            get
            {
                return DataContext as AudioVM;
            }
        }

        private CommandButtonAdapter _record;
        private PlayStopButton _playstop;
        private SaveDeleteButton _savedelete;

        public NewAudio()
        {
            InitializeComponent();

            _record = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.Record);
            _playstop = new PlayStopButton(ApplicationBar, VM);
            _savedelete = new SaveDeleteButton(ApplicationBar, VM);
        }
    }
}