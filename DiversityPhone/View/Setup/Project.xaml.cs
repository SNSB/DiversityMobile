using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;

namespace DiversityPhone.View.Setup
{
    public partial class Project : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        public Project()
        {
            InitializeComponent();

            this.ApplicationBar.Buttons.Add(
                new OKNextPageButton(VM.Messenger, Model.Page.SetupGPS, VM.Profile.IsProfileValid)
                );

        }
    }
}