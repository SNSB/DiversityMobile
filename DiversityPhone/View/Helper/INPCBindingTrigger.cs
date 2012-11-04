using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace DiversityPhone.View.Helper
{
    class INPCBindingTrigger : IDisposable
    {
        private TextBox _Control;
        private BindingExpression _Binding;

        void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_Binding != null)
                _Binding.UpdateSource();
        }


        public INPCBindingTrigger(TextBox control)
        {
            _Control = control;
            _Binding = _Control.GetBindingExpression(TextBox.TextProperty);

            _Control.TextChanged += TextChanged;
        }

        

        public void Dispose()
        {
            _Control.TextChanged -= TextChanged;

            _Control = null;
            _Binding = null;
        }
    }
}
