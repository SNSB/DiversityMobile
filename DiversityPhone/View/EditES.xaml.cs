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
using DiversityPhone.View;
using System.Threading;
using System.Globalization;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.View.Appbar;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public partial class EditES : PhoneApplicationPage
    {
        private EditESVM VM { get { return this.DataContext as EditESVM; } }

        private EditPageSaveEditButton _appbarupd;
        private EditPageDeleteButton _delete;

        private BindingExpression _DescriptionBinding;
       
        public EditES()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-De");//Zum setzen eines Default gut genug. Über UserProfile Customizable machen.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-De");
            InitializeComponent();

            _appbarupd = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _DescriptionBinding = DescTB.GetBindingExpression(TextBox.TextProperty);
            DPControlBackGround.setTBBackgroundColor(DescTB);         
        }       

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            _DescriptionBinding.UpdateSource();
            DPControlBackGround.setTBBackgroundColor(DescTB);
        }

       
    }
}