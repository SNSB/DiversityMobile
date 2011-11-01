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
        public ReactiveCommand Edit { get; private set; }

        public Event Model { get; private set; }
        public string Description { get { return Model.LocalityDescription; } }
        public Icon Icon { get; private set; }


        public EventVM(Event model, IMessageBus _messenger)
        {
            Model = model;
            this._messenger = _messenger;
            this.Icon = Icon.Event;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ =>
                        {
                            _messenger.SendMessage<Event>(Model, MessageContracts.SELECT);
                        }),
                (Edit = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<Event>(Model,MessageContracts.EDIT)),
            };
        }
    }
}
