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
using DiversityPhone.ViewModels;

namespace DiversityPhone.View
{
    public partial class TaxonManagement : PhoneApplicationPage
    {
        private TaxonManagementVM VM { get { return DataContext as TaxonManagementVM; } }
        public TaxonManagement()
        {
            InitializeComponent();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null && sender is ContextMenu)
            {
                var list = (sender as ContextMenu).DataContext as TaxonManagementVM.TaxonListVM;
                if (VM.Delete.CanExecute(list))
                    VM.Delete.Execute(list);
            }

        }
    }
}