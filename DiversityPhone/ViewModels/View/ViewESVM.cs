namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Reactive.Linq;
    using ReactiveUI.Xaml;
    using DiversityPhone.Model;
    using System.Linq;
    
    using System.Reactive.Concurrency;
    using DiversityPhone.Interface;

    public class ViewESVM : ViewPageVMBase<EventSeries>
    {
        private readonly IFieldDataService Storage;

        private ReactiveAsyncCommand getEvents = new ReactiveAsyncCommand();

        #region Commands
        public ReactiveCommand AddEvent { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand<IElementVM<EventSeries>> EditSeries { get; private set; }
        public ReactiveCommand<IElementVM<Event>> SelectEvent { get; private set; }
        #endregion

        public ReactiveCollection<EventVM> EventList { get; private set; }

        public ViewESVM(
            IFieldDataService Storage
            )
        {
            this.Storage = Storage;

            EditSeries = new ReactiveCommand<IElementVM<EventSeries>>(vm => !EventSeries.isNoEventSeries(vm.Model));
            EditSeries
                .ToMessage(MessageContracts.EDIT);

            EventList = new ReactiveCollection<EventVM>();
            EventList
                .ListenToChanges<Event, EventVM>(ev => ev.SeriesID == Current.Model.SeriesID);

            CurrentModelObservable
                .Merge(
                    from refresh in Messenger.Listen<EventMessage>(MessageContracts.REFRESH)
                    from activation in this.OnActivation().TakeUntil(CurrentModelObservable)
                    select Current.Model
                    )
                .Do(_ => EventList.Clear())
                .SelectMany(m =>
                    Storage.getEventsForSeries(m)
                    .Select(ev => new EventVM(ev))
                    .ToObservable(ThreadPoolScheduler.Instance)
                    .TakeUntil(CurrentModelObservable)
                    )
                .ObserveOnDispatcher()
                .Subscribe(EventList.Add);

            SelectEvent = new ReactiveCommand<IElementVM<Event>>();
            SelectEvent
                .ToMessage(MessageContracts.VIEW);

            AddEvent = new ReactiveCommand();
            AddEvent
                .Select(_ => new EventVM(
                    new Event()
                    {
                        SeriesID = Current.Model.SeriesID
                    }) as IElementVM<Event>)
                .ToMessage(MessageContracts.EDIT);

            Maps = new ReactiveCommand(CurrentModelObservable.Select(es => !EventSeries.isNoEventSeries(es)));
            Maps
                .Select(_ => Current.Model as ILocationOwner)
                .ToMessage(MessageContracts.VIEW);
        }
    }
}
