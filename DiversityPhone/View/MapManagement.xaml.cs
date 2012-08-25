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
using ReactiveUI;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;
using System.Reactive.Disposables;
using System.Reactive.Linq;


namespace DiversityPhone.View
{
    public partial class MapManagement : PhoneApplicationPage
    {
        IDisposable pivot_subscription = Disposable.Empty;
        private MapManagementVM VM { get { return DataContext as MapManagementVM; } }

        public MapManagement()
        {
            InitializeComponent();


        }

        private void QueryString_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var vm = VM;

            if (textbox != null && vm != null)
            {
                vm.QueryString = textbox.Text;
            }
        }

        private void currentPage_Loaded(object sender, RoutedEventArgs e)
        {
            pivot_subscription =
            VM.ObservableForProperty(x => x.IsOnlineAvailable)
                .Value()
                .StartWith(VM.IsOnlineAvailable)
                .Subscribe(online =>
                    {
                        if (online && !pivotControl.Items.Contains(onlinePivot))
                            pivotControl.Items.Add(onlinePivot);
                        else if (!online && pivotControl.Items.Contains(onlinePivot))
                            pivotControl.Items.Remove(onlinePivot);
                    });
                
        }

        private void currentPage_Unloaded(object sender, RoutedEventArgs e)
        {
            pivot_subscription.Dispose();
        }
    }
}