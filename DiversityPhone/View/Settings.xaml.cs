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
using Microsoft.Phone.Shell;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive.Disposables;
using DiversityPhone.View.Appbar;

namespace DiversityPhone.View
{
    public partial class Settings : PhoneApplicationPage
    {
        private SettingsVM VM { get { return DataContext as SettingsVM; } }       

        private ApplicationBarIconButton saveBtn, clearBtn, refreshBtn;

        private CommandButtonAdapter toSave, toClear, toRefresh;

        private bool initialized = false;

        public Settings()
        {
            InitializeComponent();            
        }      

        private void ManageTaxa_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
                VM.ManageTaxa.Execute(null);
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
                VM.UploadData.Execute(null);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                initialized = true;
                saveBtn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                toSave = new CommandButtonAdapter(VM.Save, saveBtn);

                clearBtn = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative),
                    Text = "reset",
                };
                ApplicationBar.Buttons.Add(clearBtn);
                toClear = new CommandButtonAdapter(VM.Reset, clearBtn);

                refreshBtn = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.refresh.rest.png", UriKind.Relative),
                    Text = "refresh vocabulary"
                };
                ApplicationBar.Buttons.Add(refreshBtn);
                toRefresh = new CommandButtonAdapter(VM.RefreshVocabulary, refreshBtn);              
            }            
        }      
    }
}