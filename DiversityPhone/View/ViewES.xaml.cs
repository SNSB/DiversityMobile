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
using System.Windows.Navigation;

namespace DiversityPhone
{
    public partial class ViewES : PhoneApplicationPage
    {

        private ViewESVM VM { get { return DataContext as ViewESVM; } }

        public ViewES()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.AddEvent.Execute(null);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            IEnumerable<JournalEntry> je= App.RootFrame.BackStack;
            
            je = App.RootFrame.BackStack;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IEnumerable<JournalEntry> je = App.RootFrame.BackStack;
            //Hier Backstack anpassen
        }

        private void PhoneApplicationPage_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Map_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Maps.Execute(null);
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void PhoneApplicationPage_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}