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

namespace DiversityPhone
{
    public partial class FirstTimeSetup : PhoneApplicationPage
    {
        private SetupVM VM { get { return DataContext as SetupVM; } }

        public FirstTimeSetup()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.AcceptUserData.Execute(null);
        }
    }
}