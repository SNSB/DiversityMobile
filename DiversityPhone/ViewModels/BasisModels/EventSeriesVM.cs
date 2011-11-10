namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
    using System.Collections.Generic;

    public class EventSeriesVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;
        IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Show {get;private set;}

        public EventSeries Model { get; private set; }
        public string Description { get { return Model.Description; } }
        public Icon Icon { get; private set; }

        public EventSeriesVM(EventSeries model, IMessageBus messenger)
        {
            _messenger = messenger;
            Model = model;
            EventSeries noEs = EventSeries.NoEventSeries();
            if (Model.SeriesID != noEs.SeriesID && !Model.Description.Equals(noEs.Description)) //Überprüfen auf NoEventSeries
                Icon = ViewModels.Icon.EventSeries;
            else
                Icon = ViewModels.Icon.NoEventSeries;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model,MessageContracts.SELECT)),
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model,MessageContracts.EDIT)),
                (Show = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model,MessageContracts.SHOW))
            };
        }
    }
}




