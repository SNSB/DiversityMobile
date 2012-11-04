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
using DiversityPhone.View.Helper;

namespace DiversityPhone.View
{
    public partial class EditCS : PhoneApplicationPage
    {
        public EditCSVM VM { get{return DataContext as EditCSVM;} }

        private EditPageSaveEditButton _SaveEditBtn;        
        private EditPageDeleteButton _delete;

        private INPCBindingTrigger _AccessionNumberBinding;

        public EditCS()
        {
            InitializeComponent();

            _SaveEditBtn = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _AccessionNumberBinding = new INPCBindingTrigger(AN_TB);
            DPControlBackGround.setTBBackgroundColor(AN_TB);        
        }

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {            
            DPControlBackGround.setTBBackgroundColor(AN_TB);
        }      
    }
}