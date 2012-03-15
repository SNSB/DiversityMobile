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

namespace DiversityPhone.View
{
    public partial class Settings : PhoneApplicationPage
    {
        private SettingsVM VM { get { return DataContext as SettingsVM; } }

        static ProgressIndicator Progress { get { return SystemTray.ProgressIndicator; } }

        private ApplicationBarIconButton saveBtn,clearBtn;

        private SerialDisposable setup_isbusy_subyscription = new SerialDisposable(); 

        public Settings()
        {
            InitializeComponent();

            
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Save.Execute(null);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            if (VM != null && VM.Reset.CanExecute(null))
                VM.Reset.Execute(null);
        }

        private void ManageTaxa_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null)
                VM.ManageTaxa.Execute(null);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
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
                        showClearButton(canreset);
                    });

                VM.ObservableForProperty(x => x.Setup)
                    .Select(change => change.Value)
                    .Where(setup => setup != null)
                    .Select(setup => setup.ObservableForProperty(x => x.IsBusy))
                    .Subscribe(isBusyObs =>
                    {
                        //Hide Menu Bar and Overlay, when setup is busy
                        setup_isbusy_subyscription.Disposable =
                            isBusyObs
                            .Select(x => x.Value)
                            .Subscribe(isBusy =>
                            {
                                ApplicationBar.IsVisible = !isBusy;
                                BusyOverlay.Visibility = (isBusy) ? Visibility.Visible : Visibility.Collapsed;
                                var p = Progress;
                                if(p != null)
                                {
                                    p.IsIndeterminate = p.IsVisible = isBusy;
                                }
                                this.Focus();
                            });
                    });

                showClearButton(VM.Reset.CanExecute(null));
            }

            
        }

        private void showClearButton(bool canreset)
        {
            if (canreset && !ApplicationBar.Buttons.Contains(clearBtn))
                ApplicationBar.Buttons.Add(clearBtn);
            else if(ApplicationBar.Buttons.Contains(clearBtn))
                ApplicationBar.Buttons.Remove(clearBtn);
        }
    }
}