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
using System.Reactive.Linq;
using Microsoft.Phone.Shell;


namespace DiversityPhone
{
    public partial class EditEV : PhoneApplicationPage
    {
        private EditEVVM VM { get { return DataContext as EditEVVM; } }

        public EditEV()
        {
            InitializeComponent();

            if (VM != null)
            {
                VM.Save.CanExecuteObservable
                    .DistinctUntilChanged()
                    .Subscribe(canSave => setSaveEnabled(canSave));
                setSaveEnabled(VM.Save.CanExecute(null));
            }      
        }

        private void setSaveEnabled(bool canSave)
        {
            //Named Buttons don't work :/
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = canSave;
        }

        private void LocalityTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VM != null)
                VM.LocalityDescription = LocalityTB.Text;
        }

        private void HabitatTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VM != null)
                VM.HabitatDescription = HabitatTB.Text;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Save.Execute(null);
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Edit.Execute(null);
        }
    }
}