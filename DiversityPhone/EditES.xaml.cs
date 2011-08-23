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
using Microsoft.Phone.Shell;
using System.Reactive.Linq;

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
                VM.Save.CanExecuteObservable
                    .DistinctUntilChanged()
                    .Subscribe(canSave => setSaveEnabled(canSave));
                setSaveEnabled(VM.Save.CanExecute(null));
            }

            var descBinding = DescTB.GetBindingExpression(TextBox.TextProperty);
            DescTB.TextChanged += (_, _2) => descBinding.UpdateSource();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Save.Execute(null);
        }
        void setSaveEnabled(bool state)
        {
            //Named Buttons don't work :/
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = state;
        }


        private void Cancel_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Cancel.Execute(null);
        }
    }
}