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

namespace DiversityPhone.View
{
    public partial class Settings : PhoneApplicationPage
    {
        private SettingsVM VM { get { return DataContext as SettingsVM; } }

        private ApplicationBarIconButton saveBtn,clearBtn;

        public Settings()
        {
            InitializeComponent();

            saveBtn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

            clearBtn = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.RelativeOrAbsolute),
                Text = "reset",
            };
            clearBtn.Click += Reset_Click;

            if (VM != null)
            {
                VM.Save.CanExecuteObservable
                    .Subscribe(cansave => saveBtn.IsEnabled = cansave);

                VM.Reset.CanExecuteObservable
                    .Subscribe(canreset =>
                        {
                            if (canreset)
                                ApplicationBar.Buttons.Add(clearBtn);
                            else
                                ApplicationBar.Buttons.Remove(clearBtn);
                        });
                    
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Save.Execute(null);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Reset.Execute(null);
        }

        private void ManageTaxa_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
                VM.ManageTaxa.Execute(null);
        }
    }
}