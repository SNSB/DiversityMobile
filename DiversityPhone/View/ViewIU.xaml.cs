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
    public partial class ViewIU : PhoneApplicationPage
    {
        private ViewIUVM VM { get { return DataContext as ViewIUVM; } }

        public ViewIU()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Add.Execute(null);
        }

        private void Map_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Maps.Execute(null);
        }
    }
}