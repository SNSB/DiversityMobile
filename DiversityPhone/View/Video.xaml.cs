using DiversityPhone.Services;
using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows;

namespace DiversityPhone.View
{
    public partial class NewVideo : PhoneApplicationPage
    {
        private VideoVM VM
        {
            get
            {
                return DataContext as VideoVM;
            }
        }

        private CommandButtonAdapter _record, _play, _stop;
        private SaveDeleteButton _save;
        private VideoService _Svc;

        public NewVideo()
        {
            InitializeComponent();

            _Svc = new VideoService(this.viewfinderRectangle, this.videoPlayer);

            _record = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, _Svc.Record);
            _play = new CommandButtonAdapter(ApplicationBar.Buttons[1] as IApplicationBarIconButton, _Svc.Play);
            _stop = new CommandButtonAdapter(ApplicationBar.Buttons[2] as IApplicationBarIconButton, _Svc.Stop);
            _save = new SaveDeleteButton(
                this.ApplicationBar,
                VM);

            VM.VideoService = _Svc as IVideoService;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            if (this._Svc != null)
            {
                _Svc.Dispose();
                _Svc = null;
            }
            VM.VideoService = null;
        }
    }
}