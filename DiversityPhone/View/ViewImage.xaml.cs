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
    public partial class ViewImage : PhoneApplicationPage
    {

        private ViewImageVM VM { get { return this.DataContext as ViewImageVM; } }

        private EditPageAppBarUpdater _appb;

        public ViewImage()
        {
            InitializeComponent();
            _appb = new EditPageAppBarUpdater(this.ApplicationBar, VM);
        }
    }
}