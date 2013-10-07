using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public partial class DiversityMobilePageHeader : UserControl
    {


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DiversityMobilePageHeader), new PropertyMetadata(string.Empty));



        public DiversityMobilePageHeader()
        {
            InitializeComponent();

            var b = new Binding("Text")
            {
                Source = this,
                Mode = BindingMode.OneWay
            };
            txtHeader.SetBinding(TextBlock.TextProperty, b);
        }
    }
}
