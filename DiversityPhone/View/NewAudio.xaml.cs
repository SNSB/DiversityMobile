using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public partial class NewAudio : PhoneApplicationPage
    {

        private NewAudioVM VM
        {
            get
            {
                return DataContext as NewAudioVM;
            }
        }

        public NewAudio()
        {
            InitializeComponent();
        }
    }
}