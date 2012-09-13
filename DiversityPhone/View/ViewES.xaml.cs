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
using DiversityPhone.ViewModels;
using System.Windows.Navigation;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Shell;

namespace DiversityPhone
{
    public partial class ViewES : PhoneApplicationPage
    {

        private ViewESVM VM { get { return DataContext as ViewESVM; } }

        private CommandButtonAdapter _add, _map;

        public ViewES()
        {
            InitializeComponent();

            _add = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.AddEvent);
            _map = new CommandButtonAdapter(ApplicationBar.Buttons[1] as IApplicationBarIconButton, VM.Maps);
        }

        
    }
}