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
using ReactiveUI;
using System.Reactive.Linq;

namespace DiversityPhone.View
{
    public partial class Setup : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }
        private CommandButtonAdapter _save;
        private ProgressBinding<SetupVM> progress;

        public Setup()
        {
            InitializeComponent();

            var save = new ApplicationBarIconButton(new Uri("/Images/appbar.save.rest.png", UriKind.RelativeOrAbsolute))
            {
                Text = DiversityResources.Setup_Header_ButtonSave
            };
            ApplicationBar.Buttons.Add(save);
            _save = new CommandButtonAdapter(save, VM.Save);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            while (this.NavigationService.BackStack.Any())
                this.NavigationService.RemoveBackEntry();

            if (progress == null)
                progress = new ProgressBinding<SetupVM>(VM, x => x.IsBusy);

            

            VM.ObservableForProperty(x => x.IsBusy)
                .Value()
                .Subscribe(busy => this.ApplicationBar.IsVisible = !busy);
        }
    }
}