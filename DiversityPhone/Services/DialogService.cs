namespace DiversityPhone.Services
{
    using System;
    using System.Windows;
    using DiversityPhone.Messages;
    using ReactiveUI;

    public class DialogService
    {
        private IMessageBus _messenger;

        public DialogService(IMessageBus messenger)
        {
            this._messenger = messenger;

            this._messenger.Listen<DialogMessage>()
                .Subscribe(msg => this.showDialog(msg));
        }

        private void showDialog(DialogMessage msg)
        {
            var result = MessageBox.Show(
                msg.Text,
                msg.Caption,
                (msg.Type == DialogType.OK) ? MessageBoxButton.OK : MessageBoxButton.OKCancel);

            if (msg.CallBack != null)
                msg.CallBack((result == MessageBoxResult.OK) ? DialogResult.OKYes : DialogResult.CancelNo);
        }
    }
}
