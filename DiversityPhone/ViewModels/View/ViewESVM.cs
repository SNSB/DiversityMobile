namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using DiversityPhone.Services;
    using System.Reactive.Linq;
    using ReactiveUI.Xaml;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;

    public class ViewESVM : ElementPageViewModel<EventSeries>
    {
        IList<IDisposable> _subscriptions;

        #region Services       
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand AddEvent { get; private set; }
        public ReactiveCommand FilterEvents { get; private set; }
        #endregion

        #region Properties
        public EventSeriesVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<EventSeriesVM> _Current;

        public IList<EventVM> EventList { get { return _EventList.Value; } }
        private ObservableAsPropertyHelper<IList<EventVM>> _EventList;
        #endregion



        public ViewESVM(IMessageBus messenger, Services.IOfflineStorage storage)
            : base(messenger)
        {           
            _storage = storage; 
            
            _Current = ValidModel
                .Select(es => new EventSeriesVM(Messenger, es))
                .ToProperty(this, x => x.Current);

            _EventList = ValidModel
                .Select(es => new VirtualizingReadonlyViewModelList<Event, EventVM>(
                    _storage.getEventsForSeries(es),
                    (model) => new EventVM(Messenger, model)
                ) as IList<EventVM>)
                .ToProperty(this, x => x.EventList);

            //On each Invocation of AddEvent, a new NavigationMessage is generated
            AddEvent = new ReactiveCommand();
            var newEventMessageSource =
                AddEvent
                .Timestamp()
                .CombineLatest(ValidModel, (a, b) => new { Click = a, Model = b })
                .DistinctUntilChanged(pair => pair.Click.Timestamp)
                .Select(pair => new NavigationMessage(Page.EditEV, null, ReferrerType.EventSeries,
                              (EventSeries.isNoEventSeries(pair.Model)) ? null : pair.Model.SeriesID.ToString()
                    ));
            Messenger.RegisterMessageSource(newEventMessageSource);


            FilterEvents = new ReactiveCommand();
        }
        protected override EventSeries ModelFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return _storage.getEventSeriesByID(id);
                }
                else
                    return null;
            }
            else
                return EventSeries.NoEventSeries;

        }
    }
}
