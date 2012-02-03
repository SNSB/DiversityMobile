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
using DiversityPhone.Model;



namespace DiversityPhone.View
{
    public partial class EditEV : PhoneApplicationPage
    {
        private EditEVVM VM { get { return DataContext as EditEVVM; } }
        private EditPageAppBarUpdater<Event> _appbar;

        public EditEV()
        {
            InitializeComponent();

            _appbar = new EditPageAppBarUpdater<Event>(this.ApplicationBar, VM);
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
    }
}