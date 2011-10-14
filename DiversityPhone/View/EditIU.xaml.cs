﻿using System;
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
using DiversityPhone.Model;

namespace DiversityPhone
{
    public partial class EditIU : PhoneApplicationPage
    {
        private EditIUVM VM { get { return DataContext as EditIUVM; } }

        public EditIU()
        {
            InitializeComponent();
            
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Save.Execute(null);
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Cancel.Execute(null);
        }

        private void Description_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VM != null)
                VM.Description = DescTB.Text;
        }

       

    }
}