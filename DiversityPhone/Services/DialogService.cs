namespace DiversityPhone.Services
{
    using DiversityPhone.Model;
    using DiversityPhone.ViewModels;
    using ReactiveUI;
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Windows;

    public class DialogService
    {
        private IMessageBus _messenger;

        public DialogService(
            IMessageBus messenger,
            [Dispatcher] IScheduler Dispatcher)
        {
            this._messenger = messenger;

            this._messenger.Listen<DialogMessage>()
                .ObserveOn(Dispatcher)
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
