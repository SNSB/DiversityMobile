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

namespace DiversityPhone.View
{
    public partial class EditES : PhoneApplicationPage
    {
        private EditESVM VM { get { return this.DataContext as EditESVM; } }

        private EditPageSaveEditButton _appbarupd;
        private EditPageDeleteButton _delete;

        private IList<Control> _toStore;
       
        public EditES()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-De");//Zum setzen eines Default gut genug. Über UserProfile Customizable machen.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-De");
            InitializeComponent();

            _appbarupd = new EditPageSaveEditButton(this.ApplicationBar, VM);

            _delete = new EditPageDeleteButton(ApplicationBar, VM);
            if (VM != null)
            {
                this._toStore = new List<Control> { this.DescTB };
                DPControlBackGround.adjustStoreBackgroundColors(_toStore);               
                                
            }          
        }
        private void SeriesCodeTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VM != null)
                VM.SeriesCode = SeriesCodeTB.Text;
        }

        private void DescTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VM != null)
            {
                VM.Description = DescTB.Text;
                DPControlBackGround.setTBBackgroundColor(DescTB);
            }
        }

       
    }
}