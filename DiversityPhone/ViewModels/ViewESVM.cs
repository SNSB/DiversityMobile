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

    public class ViewESVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;

        #region Services
        private INavigationService _navigation;
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

        public IList<EventVM> EventList { get { return _EventList.Value; }  }
        private ObservableAsPropertyHelper<IList<EventVM>> _EventList;
        #endregion



        public ViewESVM(IMessageBus messenger,INavigationService nav, Services.IOfflineStorage storage)
        {
            _messenger = messenger;
            _navigation = nav;
            _storage = storage;

            updateEventList();
            FilterEvents = new ReactiveCommand();

            _messenger.Listen<Event>(MessageContracts.SAVE)
               .Subscribe(ev => updateEventList());
            
            _subscriptions = new List<IDisposable>()
            {               
                (AddEvent = new ReactiveCommand())
                    .Subscribe(_ => addEvent()),
            };
        }   

        private void updateEventList()
        {
            var selectES = _messenger.Listen<EventSeries>(MessageContracts.SELECT);

            _Current = selectES
                .Select(es => new EventSeriesVM(es, _messenger))
                .ToProperty(this, x => x.Current);

            _EventList = selectES
                .Select(es => new VirtualizingReadonlyViewModelList<Event, EventVM>(
                    _storage.getEventsForSeries(es),
                    (model) => new EventVM(model, _messenger)
                ) as IList<EventVM>)
                .ToProperty(this, x => x.EventList);

        }


        private void addEvent()
        {
            _messenger.SendMessage<Event>(
                new Event()
                {
                    SeriesID = Current.Model.SeriesID
                },
                MessageContracts.EDIT
                );
            _messenger.SendMessage<Page>(Page.EditEV);
        }
    }
}
