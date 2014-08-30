using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using System;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public partial class EditEventProperty : PhoneApplicationPage
    {
        public EditPropertyVM VM { get { return DataContext as EditPropertyVM; } }

        private EditPageSaveEditButton _appb;
        private EditPageDeleteButton _delete;

        private BindingExpression _filterBinding;

        public EditEventProperty()
        {
            InitializeComponent();
            _appb = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _delete = new EditPageDeleteButton(ApplicationBar, VM);

            _filterBinding = tbFilterString.GetBindingExpression(TextBox.TextProperty);

            Observable.FromEventPattern(tbFilterString, "TextChanged")
                .Subscribe(_ => _filterBinding.UpdateSource());
        }
    }
}