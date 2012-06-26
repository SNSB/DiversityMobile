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

        private ProgressBinding<SettingsVM> progress;

        private ApplicationBarIconButton saveBtn, clearBtn, refreshBtn;

        private bool initialized = false;

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

                clearBtn = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative),
                    Text = "reset",
                };
                clearBtn.Click += Reset_Click;

                refreshBtn = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.refresh.rest.png", UriKind.Relative),
                    Text = "refresh vocabulary"
                };
                refreshBtn.Click += new EventHandler(refreshBtn_Click);


                if (VM != null)
                {
                    VM.Save.CanExecuteObservable
                        .StartWith(VM.Save.CanExecute(null))                        
                        .Subscribe(cansave => saveBtn.IsEnabled = cansave);

                    VM.Reset.CanExecuteObservable
                        .StartWith(false)//VM.Reset.CanExecute(null))
                        .Subscribe(canreset =>
                        {
                            showButtons(canreset);
                        });

                    progress = new ProgressBinding<SettingsVM>(VM, x => x.IsBusy);                   
                }
            }

            
        }

        void refreshBtn_Click(object sender, EventArgs e)
        {
            if (VM != null && VM.RefreshVocabulary.CanExecute(null))
                VM.RefreshVocabulary.Execute(null);
        }

        private void showButtons(bool canreset)
        {
            if (canreset)
            {
                if (ApplicationBar.Buttons.Count != 3)
                {
                    ApplicationBar.Buttons.Clear();
                    ApplicationBar.Buttons.Add(saveBtn);
                    ApplicationBar.Buttons.Add(refreshBtn);
                    ApplicationBar.Buttons.Add(clearBtn);
                }
            }
            else
            {
                if (ApplicationBar.Buttons.Count != 1)
                {
                    ApplicationBar.Buttons.Clear();
                    ApplicationBar.Buttons.Add(saveBtn);
                }
            }
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (VM != null)
            {
                e.Cancel = true;
                VM.NavigateBack.Execute(null);
            }
        }
    }
}