namespace DiversityPhone.ViewModels
{
    using System;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
    using System.Collections.Generic;

    public class EventVM : ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        private IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }

        public Event Model { get; private set; }
        public string Description { get { return Model.LocalityDescription; } }



        public EventVM(Event model, IMessageBus _messenger)
        {
            Model = model;
            this._messenger = _messenger;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ =>
                        {
                            _messenger.SendMessage<Event>(model, MessageContracts.SELECT);
                        })
            };

        }

    }
}
