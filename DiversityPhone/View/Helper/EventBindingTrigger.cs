namespace DiversityPhone.View
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    internal class EventBindingTrigger<T> : IDisposable where T : FrameworkElement
    {
        private T _Element;
        private BindingExpression _Binding;
        private IDisposable _Inner;

        private void EventFired(EventPattern<EventArgs> e)
        {
            if (_Binding != null)
                _Binding.UpdateSource();
        }

        public static EventBindingTrigger<TextBox> Create(TextBox tb)
        {
            return new EventBindingTrigger<TextBox>(tb, TextBox.TextProperty, "TextChanged");
        }

        public static EventBindingTrigger<PasswordBox> Create(PasswordBox pb)
        {
            return new EventBindingTrigger<PasswordBox>(pb, PasswordBox.PasswordProperty, "PasswordChanged");
        }

        public EventBindingTrigger(T element, DependencyProperty property, string eventName)
        {
            _Element = element;
            _Binding = _Element.GetBindingExpression(property);

            _Inner =
                Observable.FromEventPattern(element, eventName)
                .Subscribe(EventFired);
        }

        public void Dispose()
        {
            _Inner.Dispose();

            _Element = null;
            _Binding = null;
        }
    }
}