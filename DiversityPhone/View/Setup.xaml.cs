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
using DiversityPhone.ViewModels.Utility;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class Setup : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }
        private CommandButtonAdapter _save;

        public Setup()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            while (this.NavigationService.BackStack.Any())
                this.NavigationService.RemoveBackEntry();

            if (_save == null)
                _save = new CommandButtonAdapter(VM.Save, ApplicationBar.Buttons[0] as IApplicationBarIconButton);
        }
    }
}