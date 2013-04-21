namespace DiversityPhone.Model
{
    using System;
    using ReactiveUI;



    public static class MessengerMixin
    {
        public static IDisposable ToMessage<T>(this IObservable<T> This, string messageContract = null)
        {
            if (This == null)
                throw new ArgumentNullException("This");

            var msngr = MessageBus.Current;

            if (msngr == null) 
                throw new InvalidOperationException("No default Messenger");

            return This.Subscribe(x => msngr.SendMessage(x, messageContract));
        } 
    }
}
