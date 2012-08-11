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
using DiversityPhone.View.Appbar;

namespace DiversityPhone.View
{
    public partial class EditEventProperty : PhoneApplicationPage
    {
        public EditPropertyVM VM { get { return DataContext as EditPropertyVM; } }

        EditPageSaveEditButton _appb;
        private IList<Control> _toStore;
        private EditPageDeleteButton _delete;

        public EditEventProperty()
        {
            InitializeComponent();
            _appb = new EditPageSaveEditButton(ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _toStore = new List<Control> { LP_Type, LP_Value };
        }
    }
}