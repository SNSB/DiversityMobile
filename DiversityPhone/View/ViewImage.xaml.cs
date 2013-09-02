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
        private ViewImageVM VM
        {
            get
            {
                return DataContext as ViewImageVM;
            }
        }

        

        public ImagePage()
        {
            this.InitializeComponent();

            new EditPageDeleteButton(this.ApplicationBar, VM);
                
                
        }


    }
}