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
using DiversityPhone.Model;
using DiversityPhone.ViewModels;

namespace DiversityPhone.View
{
    public partial class EditEventProperty : PhoneApplicationPage
    {
        public EditPropertyVM VM { get { return DataContext as EditPropertyVM; } }

        EditPageAppBarUpdater<CollectionEventProperty> _appb;
        private IList<Control> _toStore;

        public EditEventProperty()
        {
            InitializeComponent();
            _toStore = new List<Control> { LP_Type, LP_Value };
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (VM != null)
                _appb = new EditPageAppBarUpdater<CollectionEventProperty>(ApplicationBar, VM);
        }
    }
}