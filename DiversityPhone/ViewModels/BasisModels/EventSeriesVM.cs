namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
    using System.Collections.Generic;
    using DiversityPhone.Services;

    public class EventSeriesVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;
        IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Edit { get; private set; }


        private EventSeries _Model;
        public EventSeries Model
        {
            get { return _Model; }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value);
            }
        }
        

        public string Description { get { return Model.Description; } }
        public Icon Icon { get; private set; }

        public EventSeriesVM( IMessageBus messenger,EventSeries model)
        {
            _messenger = messenger;
            Model = model;

            if (EventSeries.isNoEventSeries(model)) //Überprüfen auf NoEventSeries
                Icon = ViewModels.Icon.NoEventSeries;
            else
            {
                Icon = ViewModels.Icon.EventSeries;                
            }

            
            _subscriptions = new List<IDisposable>()
                {
                    (Select = new ReactiveCommand())
                        .Subscribe(_ =>
                            {
                                _messenger.SendMessage<NavigationMessage>(
                                    new NavigationMessage(
                                        Page.ViewES,
                                        (EventSeries.isNoEventSeries(Model)) ? null : Model.SeriesID.ToString()
                                        ));
                            }),
                    (Edit = new ReactiveCommand())
                        .Subscribe(_ => 
                            {
                                //NoEventSeries nicht editierbar
                                if(!EventSeries.isNoEventSeries(Model))
                                    _messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.EditES, Model.SeriesID.ToString()));
                            }),
                };
        }
    }
}




