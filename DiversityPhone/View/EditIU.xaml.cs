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
using DiversityPhone.Model;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Shell;
using DiversityPhone.View.Helper;

namespace DiversityPhone.View
{
    public partial class EditIU : PhoneApplicationPage
    {
        private EditIUVM VM { get { return DataContext as EditIUVM; } }

        private EditPageSaveEditButton _appb;
        private EditPageDeleteButton _delete;
        private INPCBindingTrigger _querystring;


        public EditIU()
        {
            InitializeComponent();

            _appb = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _querystring = new INPCBindingTrigger(QueryString);
        }        
    }
}