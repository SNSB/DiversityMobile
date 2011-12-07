namespace DiversityPhone.ViewModels
{
    using System;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
    using System.Collections.Generic;
    using DiversityPhone.Services;

    public class EventVM : ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        private IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Edit { get; private set; }

        public Event Model { get; private set; }
        public string Description { get { return Model.LocalityDescription; } }
        public Icon Icon { get; private set; }


        public EventVM(IMessageBus _messenger,Event model)
        {
            Model = model;
            this._messenger = _messenger;
            this.Icon = Icon.Event;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ =>
                        {
                            _messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.ViewEV, Model.EventID.ToString()));
                        }),
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => 
                        {
                            _messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.EditEV, Model.EventID.ToString()));
                        }),
            };
        }
    }
}
