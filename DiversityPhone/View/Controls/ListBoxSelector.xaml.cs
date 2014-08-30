using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public partial class ListBoxSelector : UserControl
    {
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ListBoxSelector), new PropertyMetadata(null, (sender, args) => { }));

        public int SelectionWidth
        {
            get { return (int)GetValue(SelectionWidthProperty); }
            set { SetValue(SelectionWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionWidthProperty =
            DependencyProperty.Register("SelectionWidth", typeof(int), typeof(ListBoxSelector), new PropertyMetadata(0));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ListBoxSelector), new PropertyMetadata(null, (sender, args) => { }));

        public bool IsSelecting
        {
            get { return (bool)GetValue(IsSelectingProperty); }
            set { SetValue(IsSelectingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelecting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectingProperty =
            DependencyProperty.Register("IsSelecting", typeof(bool), typeof(ListBoxSelector), new PropertyMetadata(false, (sender, args) =>
            {
                if (sender is ListBoxSelector)
                {
                    (sender as ListBoxSelector).SetSelectingState((bool)args.NewValue);
                }
            }));

        public ListBoxSelector()
        {
            InitializeComponent();

            SelectionMode.CurrentStateChanged += (sender, args) =>
            {
                touchableEdge.IsHitTestVisible = (args.NewState == Normal);
            };

            IsSelecting = false;

            var itemsSourceDefaultBinding = new Binding()
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath("DataContext.SelectableItems"),
                Source = this
            };
            this.SetBinding(ItemsSourceProperty, itemsSourceDefaultBinding);

            var isSelectingDefaultBinding = new Binding()
            {
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath("DataContext.IsSelecting"),
                Source = this
            };
            this.SetBinding(IsSelectingProperty, isSelectingDefaultBinding);
        }

        private void EnterSelecting(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.IsSelecting = true;
        }

        private void SetSelectingState(bool value)
        {
            VisualStateManager.GoToState(this, (value) ? Selecting.Name : Normal.Name, true);
        }
    }
}