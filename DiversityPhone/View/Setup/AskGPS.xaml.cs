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
    public partial class AskGPS : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        private CommandButtonAdapter Save;

        public AskGPS()
        {
            InitializeComponent();

            var saveBtn = new ApplicationBarIconButton(new Uri("/Images/appbar.save.rest.png", UriKind.Relative))
                {
                    Text = DiversityResources.Settings_Header_ButtonSave
                };

            Save = new CommandButtonAdapter(saveBtn, VM.Save);

            this.ApplicationBar.Buttons.Add(saveBtn);
        }
    }
}