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
    public partial class EditES : PhoneApplicationPage
    {
        private EditESVM VM { get { return this.DataContext as EditESVM; } }

        public EditES()
        {
            InitializeComponent();
            if (VM != null)
            {
                VM.Save.CanExecuteObservable.Subscribe(canSave => this.SaveButton.IsEnabled = canSave);
            }
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
    }
}