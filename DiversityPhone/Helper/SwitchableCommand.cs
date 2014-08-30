using System;
using System.Windows.Input;

namespace DiversityPhone
{
    public class SwitchableCommand : ICommand
    {
        private bool _IsExecutable;

        public bool IsExecutable
        {
            get
            {
                return _IsExecutable;
            }
            set
            {
                _IsExecutable = value;
                RaiseCanExecuteChanged();
            }
        }

        private Action<object> executeAction;

        public SwitchableCommand(Action executableAction)
            : this(o => executableAction())
        {
            if (executeAction == null)
            {
                throw new ArgumentNullException("executeAction");
            }
        }

        public SwitchableCommand(Action<object> executeAction)
        {
            if (executeAction == null)
            {
                throw new ArgumentNullException("executeAction");
            }
            this.executeAction = executeAction;
        }

        public bool CanExecute(object parameter)
        {
            return IsExecutable;
        }

        public event EventHandler CanExecuteChanged;

        private void RaiseCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            this.executeAction(parameter);
        }
    }
}