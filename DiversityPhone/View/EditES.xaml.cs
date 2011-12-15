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

namespace DiversityPhone
{
    public partial class EditES : PhoneApplicationPage
    {
        private EditESVM VM { get { return this.DataContext as EditESVM; } }

        private IList<Control> _toStore;
       
        public EditES()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-De");//Zum setzen eines Default gut genug. Über UserProfile Customizable machen.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-De");
            InitializeComponent();
            if (VM != null)
            {
                this._toStore = new List<Control> { this.DescTB };
                this.adjustStoreBackgroundColors();
                
                VM.Save.CanExecuteObservable
                    .DistinctUntilChanged()
                    .Subscribe(canSave => setSaveEnabled(canSave));
                setSaveEnabled(VM.Save.CanExecute(null));
                VM.ObservableForProperty(vm => vm.Editable)
                    .Select(change => change.Value)
                    .StartWith(VM.Editable)
                    .Subscribe(value => adjustApplicationBar(value));
            }          
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Save.Execute(null);
        }

        void setSaveEnabled(bool state)
        {
            //Named Buttons don't work :/
            if (ApplicationBar.Buttons.Count>0)
            {
                if (VM.Editable == false)
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                else
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = state;
                //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = state;
            }
        }

        private void adjustApplicationBar(bool editable)
        {
            {
                this.ApplicationBar.Buttons.Clear();
                if (editable == true)
                {
                    ApplicationBarIconButton save = new ApplicationBarIconButton();
                    save.Text = "save";
                    save.IconUri =new Uri("/Images/appbar.save.rest.png",UriKind.Relative);
                    save.Click += new EventHandler(Save_Click);
                    ApplicationBar.Buttons.Add(save);
                    ApplicationBarIconButton delete = new ApplicationBarIconButton();
                    delete.Text = "delete";
                    delete.IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative);
                    delete.Click += new EventHandler(Delete_Click);
                    ApplicationBar.Buttons.Add(delete);
                }
                else
                {
                    ApplicationBarIconButton edit = new ApplicationBarIconButton();
                    edit.Text = "edit";
                    edit.IconUri = new Uri("/Images/appbar.edit.rest.png", UriKind.Relative);
                    edit.Click += new EventHandler(Edit_Click);
                    ApplicationBar.Buttons.Add(edit);
                }
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
                this.setTBBackgroundColor(DescTB);//Kann man die binden?
            }
        }

        public void adjustStoreBackgroundColors()
        {
            if (_toStore != null)
            {
                foreach (Control c in _toStore)
                {
                    if (c.GetType().Equals(typeof(TextBox)))
                    {
                        TextBox tb = (TextBox)c;
                        this.setTBBackgroundColor(tb);
                    }
                }
            }
        }

        public void setTBBackgroundColor(TextBox tb)
        {
            if (tb.Text == String.Empty || tb.Text == null)
                tb.Background = DPColors.INPUTMISSSING;
            else
                tb.Background = DPColors.STANDARD;
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            this.VM.SeriesEnd = DateTime.Now;
            EventSeries.Actual = null;
            //Georeferenzierung einfügen
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            if (VM != null)
            {
                VM.ToggleEditable.Execute(null);
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {

            if (VM != null)
                VM.Delete.Execute(null);
        }
 
    }
}