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
using System.Windows.Data;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;
using DiversityPhone.View.Appbar;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class EditCS : PhoneApplicationPage
    {
        BindingExpression anBinding;

        public EditCSVM VM { get{return DataContext as EditCSVM;} }

        private EditPageSaveEditButton _appbarupd;
        private IList<Control> _toStore;
        private EditPageDeleteButton _delete;

        public EditCS()
        {
            InitializeComponent();

            _appbarupd = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            anBinding =  AN_TB.GetBindingExpression(TextBox.TextProperty);
            DPControlBackGround.setTBBackgroundColor(AN_TB);        
        }

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            anBinding.UpdateSource();
            DPControlBackGround.setTBBackgroundColor(AN_TB);
        }      
    }
}