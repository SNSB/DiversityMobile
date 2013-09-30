using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using ReactiveUI;
using Ninject;
using System.Reactive.Linq;
using DiversityPhone.Services;
using DiversityPhone.Interface;

namespace DiversityPhone.View
{
    public partial class ImportExport : PhoneApplicationPage
    {
        private ImportExportVM VM { get { return DataContext as ImportExportVM; } }


        private ApplicationBarIconButton DownloadButton, UploadButton, RestoreButton, DeleteButton, TakeButton, RefreshButton;
        private CommandButtonAdapter _Take, _Delete, _Upload, _Download, _Restore, _Refresh;

        public ImportExport()
        {
            InitializeComponent();
            DownloadButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.download.rest.png", UriKind.Relative),
                Text = DiversityResources.ImportExport_Button_Download
            };
            _Download = new CommandButtonAdapter(DownloadButton, VM.DownloadSnapshot);

            UploadButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.upload.rest.png", UriKind.Relative),
                Text = DiversityResources.ImportExport_Button_Upload
            };
            _Upload = new CommandButtonAdapter(UploadButton, VM.UploadSnapshot);

            TakeButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.add.rest.png", UriKind.Relative),
                Text=DiversityResources.ImportExport_Button_Take
            };
            _Take = new CommandButtonAdapter(TakeButton, VM.TakeSnapshot);

            DeleteButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative),
                Text = DiversityResources.ImportExport_Button_Delete
            };
            _Delete = new CommandButtonAdapter(DeleteButton, VM.DeleteSnapshot);

            RestoreButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/MetroIcons/white/rew.png", UriKind.Relative),
                Text=DiversityResources.ImportExport_Button_Restore
            };
            _Restore = new CommandButtonAdapter(RestoreButton, VM.RestoreSnapshot);

            RefreshButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.sync.rest.png", UriKind.Relative),
                Text = DiversityResources.ImportExport_Button_RefreshRemote
            };
            _Refresh = new CommandButtonAdapter(RefreshButton, VM.RefreshRemote);

            var isBusy = 
                VM.WhenAny(x => x.IsBusy, x => x.GetValue())
                    .DistinctUntilChanged()
                    .Publish().RefCount();

            var progressPercent = 
                VM.WhenAny(x => x.ProgressPercentage, x => x.GetValue());

            isBusy
                .Subscribe(b => this.ApplicationBar.IsVisible = !b);

            

            var Notifications = App.Kernel.Get<INotificationService>();

            isBusy
                .Where(b => b)
                .Select(_ => progressPercent
                    .Select(p => new ProgressState(p, string.Empty))
                    .TakeUntil(isBusy.Where(b => !b)))
                .Subscribe(Notifications.showProgress);

            VM.WhenAny(x => x.CurrentPivot, x => x.GetValue())
                .Subscribe(UpdateAppBarForPivot);
        }

        private void UpdateAppBarForPivot(ImportExportVM.Pivot obj)
        {
            this.ApplicationBar.Buttons.Clear();
            switch (obj)
            {
                case ImportExportVM.Pivot.local:
                    this.ApplicationBar.Buttons.Add(TakeButton);
                    this.ApplicationBar.Buttons.Add(RestoreButton);
                    this.ApplicationBar.Buttons.Add(DeleteButton);
                    this.ApplicationBar.Buttons.Add(UploadButton);
                    break;
                case ImportExportVM.Pivot.remote:
                    this.ApplicationBar.Buttons.Add(RefreshButton);
                    this.ApplicationBar.Buttons.Add(DownloadButton);
                    break;
                default:
                    break;
            }
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = VM;
            if (vm != null)
            {
                e.Cancel = vm.IsBusy;
            }
        }
    }
}