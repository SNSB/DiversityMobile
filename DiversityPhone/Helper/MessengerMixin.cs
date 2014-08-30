namespace DiversityPhone.Model
{
    using ReactiveUI;
    using System;
    using System.Diagnostics.Contracts;

    public static class MessengerMixin
    {
        public static IDisposable ToMessage<T>(this IObservable<T> This, IMessageBus Messenger, string messageContract = null)
        {
            Contract.Requires(This != null);
            Contract.Requires(Messenger != null);

            return This.Subscribe(x => Messenger.SendMessage(x, messageContract));
        }
    }
}