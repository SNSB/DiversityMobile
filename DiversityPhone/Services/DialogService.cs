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
using ReactiveUI;
using DiversityPhone.Messages;

namespace DiversityPhone.Services
{
    public class DialogService
    {
        IMessageBus _messenger;
        public DialogService(IMessageBus messenger)
        {
            _messenger = messenger;

            _messenger.Listen<DialogMessage>()
                .Subscribe(msg => showDialog(msg));
        }

        private void showDialog(DialogMessage msg)
        {
            MessageBox.Show(msg.Text,msg.Caption,
                (msg.Type == DialogType.OK) ? MessageBoxButton.OK : MessageBoxButton.OKCancel);
        }
    }
}
