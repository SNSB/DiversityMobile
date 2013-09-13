using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View.Setup
{
    public partial class Vocabulary : PhoneApplicationPage
    {
        public Vocabulary()
        {
            InitializeComponent();            
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            while (this.NavigationService.BackStack.Any())
                this.NavigationService.RemoveBackEntry();
        }
    }
}