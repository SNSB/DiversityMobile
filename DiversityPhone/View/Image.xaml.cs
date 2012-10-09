using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;
using System.Windows;
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class ImagePage : PhoneApplicationPage
    {        
        private ImageVM VM
        {
            get
            {
                return DataContext as ImageVM;
            }
        }

        private CommandButtonAdapter _takeButton;
        private EditPageSaveEditButton _editSaveButton;

        public ImagePage()
        {
            this.InitializeComponent();

            _takeButton = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.Take);
            _editSaveButton = new EditPageSaveEditButton(ApplicationBar, VM);
        }


    }
}