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
using System.Reactive.Disposables;

namespace DiversityPhone.View.Appbar
{
    public class CommandButtonAdapter : IDisposable
    {
        public IApplicationBarIconButton Button { get; private set; }

        private ICommand _Command;
        public ICommand Command 
        {
            get
            {
                return _Command;
            }
            set
            {
                if(_Command != value)
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

        public CommandButtonAdapter(IApplicationBar appbar, ICommand command = null)
            : this(new ApplicationBarIconButton() { IconUri = new Uri("/Images/appbar.add.rest.png", UriKind.RelativeOrAbsolute), Text = "test" }, command)
        {
            appbar.Buttons.Add(this.Button);
        }

        public CommandButtonAdapter(IApplicationBarIconButton button, ICommand command = null)
        {           
            if (button == null)
                throw new ArgumentNullException("button");
            Button = button;
            Command = command; 
        }

        void Click(object sender, EventArgs e)
        {
            if(Button != null && Command != null && Command.CanExecute(null))
                Command.Execute(null);
        }

        void  CanExecuteChanged(object sender, EventArgs e)
        {
            if(Button != null && Command != null)
                Button.IsEnabled = Command.CanExecute(null);
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
