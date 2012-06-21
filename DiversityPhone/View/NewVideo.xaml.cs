using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using ReactiveUI.Xaml;
using DiversityPhone.View.Appbar;

namespace DiversityPhone.View
{
    public partial class NewVideo : PhoneApplicationPage
    {

        private NewVideoVM VM
        {
            get
            {
                return DataContext as NewVideoVM;
            }
        }

        private NewVideoAppBarUpdater _appbar;

        public NewVideo()
        {
            InitializeComponent();
            _appbar = new NewVideoAppBarUpdater(this.ApplicationBar, VM);          
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Dispose of camera and media objects.
            VM.clean();

            base.OnNavigatedFrom(e);
        }


    }
}