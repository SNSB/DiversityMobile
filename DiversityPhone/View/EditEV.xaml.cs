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
using DiversityPhone.View.Appbar;
using System.Windows.Data;



namespace DiversityPhone.View
{
    public partial class EditEV : PhoneApplicationPage
    {
        private EditEVVM VM { get { return DataContext as EditEVVM; } }
        private EditPageSaveEditButton _appbar;        
        private EditPageDeleteButton _delete;
        private BindingExpression _LocalityBinding;
       

        public EditEV()
        {
            InitializeComponent();

            _appbar = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);
            _LocalityBinding = LocalityTB.GetBindingExpression(TextBox.TextProperty);

            DPControlBackGround.setTBBackgroundColor(LocalityTB);
        }      

        private void LocalityTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            _LocalityBinding.UpdateSource();
            DPControlBackGround.setTBBackgroundColor(LocalityTB);            
        }
    }
}