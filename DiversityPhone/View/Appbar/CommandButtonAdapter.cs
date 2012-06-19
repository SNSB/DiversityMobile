using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View.Appbar
{
    public class CommandButtonAdapter : IDisposable
    {
        private bool _disposed = false;
        private ICommand _cmd;
        private IApplicationBarIconButton _btn;

        public CommandButtonAdapter(ICommand command, IApplicationBarIconButton button)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (button == null)
                throw new ArgumentNullException("button");
            _cmd = command;
            _btn = button;

            _cmd.CanExecuteChanged += new EventHandler(CanExecuteChanged);
            _btn.Click += new EventHandler(Click);

            CanExecuteChanged(null, null);
        }

        void Click(object sender, EventArgs e)
        {
            if (!_disposed && _cmd.CanExecute(null))
                _cmd.Execute(null);
        }

        void  CanExecuteChanged(object sender, EventArgs e)
        {
            if (!_disposed)
                _btn.IsEnabled = _cmd.CanExecute(null);
        }       

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cmd.CanExecuteChanged -= CanExecuteChanged;
                _cmd = null;
                _btn.Click -= Click;
                _btn = null;
            }
        }
    }
}
