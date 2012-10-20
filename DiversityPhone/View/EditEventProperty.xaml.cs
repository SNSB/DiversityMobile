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
using System.Reactive.Linq;

namespace DiversityPhone.View
{
    public partial class EditEventProperty : PhoneApplicationPage
    {
        public EditPropertyVM VM { get { return DataContext as EditPropertyVM; } }

        EditPageSaveEditButton _appb;
        private EditPageDeleteButton _delete;

        public EditEventProperty()
        {
            InitializeComponent();
            _appb = new EditPageSaveEditButton(ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            Observable.FromEventPattern(tbFilterString, "TextChanged")                
                .Select(_ => tbFilterString.Text)                                
                .Subscribe(filter => VM.FilterString = filter);
        }

        
    }
}