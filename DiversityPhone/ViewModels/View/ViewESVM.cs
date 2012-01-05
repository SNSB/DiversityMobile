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

    public class ViewESVM : PageViewModel
    {
        IList<IDisposable> _subscriptions;

        #region Services

        IMessageBus _messenger;
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
        {
            _messenger = messenger;
            _storage = storage;


            var rawModel = StateObservable
                .Select(s => EventSeriesFromContext(s.Context));
            var modelDeleted = rawModel.Where(es => es == null);
            var validModel = rawModel.Where(es => es != null);

            _messenger.RegisterMessageSource(modelDeleted.Select(_ => Message.NavigateBack));


            _Current = validModel
                .Select(es => new EventSeriesVM(_messenger, es))
                .ToProperty(this, x => x.Current);

            _EventList = validModel
                .Select(es => new VirtualizingReadonlyViewModelList<Event, EventVM>(
                    _storage.getEventsForSeries(es),
                    (model) => new EventVM(_messenger, model)
                ) as IList<EventVM>)
                .ToProperty(this, x => x.EventList);

            //On each Invocation of AddEvent, a new NavigationMessage is generated
            AddEvent = new ReactiveCommand();
            var newEventMessageSource =
                AddEvent
                .Timestamp()
                .CombineLatest(validModel, (a, b) => new { Click = a, Model = b })
                .DistinctUntilChanged(pair => pair.Click.Timestamp)
                .Select(pair => new NavigationMessage(Page.EditEV, null, ReferrerType.EventSeries,
                              (EventSeries.isNoEventSeries(pair.Model)) ? null : pair.Model.SeriesID.ToString()
                    ));
            _messenger.RegisterMessageSource(newEventMessageSource);


            FilterEvents = new ReactiveCommand();
        }
        private EventSeries EventSeriesFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
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
