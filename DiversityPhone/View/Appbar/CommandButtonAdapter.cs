using Microsoft.Phone.Shell;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiversityPhone.View.Appbar
{
    public class CommandButtonAdapter : IDisposable
    {
        public enum Mode
        {
            HideButton,
            DisableButton
        }

        private IApplicationBar _AppBar;

        private IApplicationBarIconButton _Button;

        public IApplicationBarIconButton Button
        {
            get
            {
                return _Button;
            }
            private set
            {
                if (_Button != null)
                {
                    _Button.Click -= this.Click;
                    if (_AppBar != null)
                    {
                        _AppBar.Buttons.Remove(_Button);
                    }
                }
                _Button = value;
                if (_Button != null && Command != null)
                {
                    _Button.Click += this.Click;
                    this.CanExecuteChanged(null, null);
                }
            }
        }

        public Mode HideMode { get; private set; }

        private ICommand _Command;

        public ICommand Command
        {
            get
            {
                return _Command;
            }
            set
            {
                if (_Command != value)
                {
                    _Command = value;
                    if (_Command != null && Button != null)
                    {
                        _command_handle.Disposable = new CompositeDisposable(
                            Disposable.Create(() => _Command.CanExecuteChanged -= CanExecuteChanged),
                            Disposable.Create(() => Button.Click -= Click)
                            );
                        _Command.CanExecuteChanged += CanExecuteChanged;
                        Button.Click += Click;
                        CanExecuteChanged(null, null);
                    }
                    else
                        _command_handle.Dispose();
                }
            }
        }

        private SerialDisposable _command_handle = new SerialDisposable();

        public CommandButtonAdapter(
            IApplicationBar appbar,
            IApplicationBarIconButton button,
            Mode hideMode = Mode.DisableButton,
            ICommand command = null)
        {
            if (appbar == null && hideMode != Mode.DisableButton)
                throw new ArgumentException("Without an appbar reference I can only disable buttons not hide them");
            if (button == null)
                throw new ArgumentNullException("button");

            _AppBar = appbar;
            HideMode = hideMode;
            Button = button;
            Command = command;

            if (_AppBar != null &&
               HideMode == Mode.DisableButton)
            {
                if (!_AppBar.Buttons.Contains(Button))
                {
                    _AppBar.Buttons.Add(Button);
                }
            }
        }

        public CommandButtonAdapter(IApplicationBarIconButton button, ICommand command = null)
            : this(appbar: null, hideMode: Mode.DisableButton, button: button, command: command)
        {
        }

        private void Click(object sender, EventArgs e)
        {
            if (Button != null && Command != null && Command.CanExecute(null))
                Command.Execute(null);
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (Command == null || Button == null)
            {
                return;
            }

            var canExecute = Command.CanExecute(null);

            switch (HideMode)
            {
                case Mode.DisableButton:
                    Button.IsEnabled = canExecute;
                    break;

                case Mode.HideButton:
                    if (_AppBar != null)
                    {
                        var buttonVisible = _AppBar.Buttons.Contains(Button);
                        if (canExecute && !buttonVisible)
                        {
                            Button.IsEnabled = true;
                            _AppBar.Buttons.Add(Button);
                        }
                        else if (!canExecute && buttonVisible)
                        {
                            _AppBar.Buttons.Remove(Button);
                        }
                        AdjustAppBarSize();
                    }
                    break;
            }
        }

        private void AdjustAppBarSize()
        {
            if (_AppBar.Buttons.Count == 0)
            {
                if (_AppBar.MenuItems.Count > 0)
                {
                    _AppBar.Mode = ApplicationBarMode.Minimized;
                }
                else
                {
                    _AppBar.IsVisible = false;
                }
            }
            else
            {
                _AppBar.Mode = ApplicationBarMode.Default;
                _AppBar.IsVisible = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _command_handle.Dispose();
            Button = null;
        }
    }
}