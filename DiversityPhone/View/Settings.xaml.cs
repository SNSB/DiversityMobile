using DiversityPhone.Services;
using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ninject;
using System;
using System.Reflection;
using System.Windows;

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

            version_info.Text = GetVersionNumber();

            this.SigninButton.SessionChanged += (s, args) =>
            {
                var cloud = App.Kernel.Get<CloudStorageService>();
                cloud.Session = args.Session;
            };
        }

        private void ManageTaxa_Click()
        {
            if (VM != null)
                VM.ManageTaxa.Execute(null);
        }

        private void Upload_Click()
        {
            if (VM != null)
                VM.UploadData.Execute(null);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                initialized = true;

                saveBtn = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.save.rest.png", UriKind.Relative),
                    Text = DiversityResources.Settings_Header_ButtonSave
                };
                ApplicationBar.Buttons.Add(saveBtn);
                toSave = new CommandButtonAdapter(saveBtn, VM.Save);

                refreshBtn = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.refresh.rest.png", UriKind.Relative),
                    Text = DiversityResources.Settings_Header_ButtonRefresh
                };
                ApplicationBar.Buttons.Add(refreshBtn);
                toRefresh = new CommandButtonAdapter(refreshBtn, VM.RefreshVocabulary);

                clearBtn = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative),
                    Text = DiversityResources.Settings_Header_ButtonClear,
                };
                ApplicationBar.Buttons.Add(clearBtn);
                toClear = new CommandButtonAdapter(clearBtn, VM.Reset);

                var manageTaxa = new ApplicationBarMenuItem()
                {
                    Text = DiversityResources.Settings_Header_ManageTaxa
                };
                manageTaxa.Click += (s, args) => ManageTaxa_Click();
                ApplicationBar.MenuItems.Add(manageTaxa);

                var uploadData = new ApplicationBarMenuItem()
                {
                    Text = DiversityResources.Settings_Header_UploadData
                };
                uploadData.Click += (s, args) => Upload_Click();
                ApplicationBar.MenuItems.Add(uploadData);

                var downloadData = new ApplicationBarMenuItem()
                {
                    Text = DiversityResources.Settings_Header_DownloadData
                };
                downloadData.Click += (s, args) => Download_Click();
                ApplicationBar.MenuItems.Add(downloadData);

                var importExport = new ApplicationBarMenuItem()
                {
                    Text = DiversityResources.Settings_Menu_ImExport
                };
                importExport.Click += (s, args) => ImportExport();
                ApplicationBar.MenuItems.Add(importExport);
            }
        }

        private void ImportExport()
        {
            var vm = VM;
            if (vm != null && vm.ImportExport.CanExecute(null))
                vm.ImportExport.Execute(null);
        }

        private void Download_Click()
        {
            var vm = VM;
            if (vm != null && vm.DownloadData.CanExecute(null))
                vm.DownloadData.Execute(null);
        }

        private static string GetVersionNumber()
        {
            var asm = Assembly.GetExecutingAssembly();
            var parts = asm.FullName.Split(',');
            return parts[1].Split('=')[1];
        }

        private void PrivacyPolicy_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}