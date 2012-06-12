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
    public partial class SelectNewMMO : PhoneApplicationPage
    {

        private SelectNewMMOVM VM
        {
            get
            {
                return DataContext as SelectNewMMOVM;
            }
        }

        public SelectNewMMO()
        {
            InitializeComponent();
        }


        private void btnNewPhoto_Click(object sender, EventArgs e)
        {
            VM.SelectImage.Execute(null);
        }


        private void btnNewAudio_Click(object sender, EventArgs e)
        {
            VM.SelectAudio.Execute(null);
        }

        private void btnNewVideo_Click(object sender, EventArgs e)
        {
            VM.SelectVideo.Execute(null);
        }



    }
}