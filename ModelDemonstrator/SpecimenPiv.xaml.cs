using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace ModelDemonstrator
{
    public partial class SpecimenPiv : PhoneApplicationPage
    {
        public SpecimenPiv()
        {
            InitializeComponent();
        }

        private void canvas1_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/IUPiv.xaml", UriKind.Relative));
        }
    }
}