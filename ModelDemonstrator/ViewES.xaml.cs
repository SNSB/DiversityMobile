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
    public partial class ViewES : PhoneApplicationPage
    {

        bool editable;

        public ViewES(bool editable)
        {
            InitializeComponent();
            this.editable = editable;
            if (editable == false)
            {
            }
            else
            {
            }
        }


        private void Edit_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditES.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}