using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;
using DiversityPhone.View.Appbar;

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

        private NewAudioAppBarUpdater _appbar;

        public NewAudio()
        {
            InitializeComponent();
            _appbar = new NewAudioAppBarUpdater(this.ApplicationBar, VM);
        }
    }
}