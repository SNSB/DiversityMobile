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

namespace DiversityPhone.View
{
    public partial class EditCS : PhoneApplicationPage
    {
        BindingExpression anBinding;

        public EditCSVM VM { get{return DataContext as EditCSVM;} }

        private EditPageAppBarUpdater _appbarupd;
        private IList<Control> _toStore;

        public EditCS()
        {
            InitializeComponent();

            _appbarupd = new EditPageAppBarUpdater(this.ApplicationBar, VM);

            anBinding =  AN_TB.GetBindingExpression(TextBox.TextProperty);
            this._toStore = new List<Control> { this.AN_TB };
            DPControlBackGround.adjustStoreBackgroundColors(_toStore);  
        }

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            anBinding.UpdateSource();
            DPControlBackGround.setTBBackgroundColor(AN_TB);
        }      
    }
}