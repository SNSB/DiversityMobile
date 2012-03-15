using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace DiversityPhone.Controls
{
    public partial class CommandNotAvailableLabel : UserControl
    {
        public CommandNotAvailableLabel()
        {
            InitializeComponent();

            var b = new Binding("Text")
            {
                Source = this,
                Mode = BindingMode.OneWay                
            };
            textField.SetBinding(TextBlock.TextProperty,b);


        }



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CommandNotAvailableLabel), new PropertyMetadata("Command not available"));



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

                    if(sender == null)
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

            if(command != null && !command.CanExecute(null))
            {
                contentPanel.Visibility = Visibility.Visible;
            }
            else
                contentPanel.Visibility = Visibility.Collapsed;
        }
    }
}
