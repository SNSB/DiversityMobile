using System.Windows;

namespace DiversityPhone.View
{
    public class ConditionalLabel : InfoLabel
    {
        public ConditionalLabel()
        {
            Visibility = System.Windows.Visibility.Collapsed;
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(ConditionalLabel), new PropertyMetadata(false, (s, args) =>
                {
                    if (s is ConditionalLabel)
                    {
                        var t = s as ConditionalLabel;
                        t.Visibility = ((bool)args.NewValue) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }));
    }
}