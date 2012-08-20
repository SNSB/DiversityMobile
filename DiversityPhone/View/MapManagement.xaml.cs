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

using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using System.Windows.Data;


namespace DiversityPhone.View
{
    public partial class MapManagement : PhoneApplicationPage
    {
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
    }
}