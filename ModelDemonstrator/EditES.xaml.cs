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

namespace ModelDemonstrator
{
    public partial class EditES : PhoneApplicationPage
    {
        public EditES()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, EventArgs e)
        {

        }

        private void Cancel_Click(object sender, EventArgs e)
        {

        }

        private void Delete_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void btnStore_Click(object sender, RoutedEventArgs e)
        {
            LocationService ls = LocationService.INSTANCE;
            ls.Initialize(this.textBox1);
            ls.Start();
        }
    }
}