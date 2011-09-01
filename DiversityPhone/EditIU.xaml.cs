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
using DiversityService.Model;

namespace DiversityPhone
{
    public partial class EditIU : PhoneApplicationPage
    {
        private EditIUVM VM { get { return DataContext as EditIUVM; } }

        public EditIU()
        {
            InitializeComponent();
        }

        private void TaxonGroup_Changed(object sender, SelectionChangedEventArgs e)
        {
            var newSelection = e.AddedItems.Count > 0 ? e.AddedItems[0] as Term : null;
            if (VM != null)
                VM.TaxonomicGroup = newSelection;
                
        }
    }
}