using System;
using System.Windows;
using System.Windows.Input;

namespace DiversityPhone.View
{
    public partial class CommandNotAvailableLabel : ConditionalLabel
    {
        public CommandNotAvailableLabel()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandNotAvailableLabel), new PropertyMetadata(null, (s, args) =>
            {
                var sender = s as CommandNotAvailableLabel;

                if (sender == null)
                    return;

                var oldC = args.OldValue as ICommand;
                var newC = args.NewValue as ICommand;
                if (oldC != null)
                    oldC.CanExecuteChanged -= sender.commandCanExecuteChanged;
                if (newC != null)
                {
                    newC.CanExecuteChanged += sender.commandCanExecuteChanged;
                    sender.commandCanExecuteChanged(newC, null);
                }
            }));

        private void commandCanExecuteChanged(object sender, EventArgs args)
        {
            var command = sender as ICommand;
            var isVisible = command != null && !command.CanExecute(null);

            this.Dispatcher.BeginInvoke(() => this.IsVisible = isVisible);
        }
    }
}