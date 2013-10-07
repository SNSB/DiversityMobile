using Microsoft.Phone.Shell;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiversityPhone.View.Appbar
{
    public class SwitchingCommandButtonAdapter : CommandButtonAdapter
    {
        protected class ButtonState
        {
            public Uri URI { get; set; }
            public string Text { get; set; }
            public ICommand Command { get; set; }
        }

        private ButtonState _CurrentState;
        protected ButtonState CurrentState
        {
            get
            {
                return _CurrentState;
            }
            set
            {
                if (_CurrentState != value)
                {
                    _CurrentState = value;
                    if (_CurrentState != null)
                    {
                        Button.IconUri = _CurrentState.URI;
                        Button.Text = _CurrentState.Text;
                        Command = _CurrentState.Command;
                    }
                    else
                    {
                        Button.IconUri = null;
                        Button.Text = string.Empty;
                        Command = null;
                    }
                }
            }
        }

        public SwitchingCommandButtonAdapter(IApplicationBar appbar)
            : base(appbar, null)
        {
            
        }
    }
}
