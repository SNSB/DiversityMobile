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
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using Funq;
    using System.Reactive.Concurrency;

    public class ViewESVM : ViewPageVMBase<EventSeries>
    {
        private ReactiveAsyncCommand getEvents = new ReactiveAsyncCommand();  

        private IFieldDataService Storage;

        #region Commands
        public ReactiveCommand AddEvent { get; private set; }        
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand<IElementVM<EventSeries>> EditSeries { get; private set; }
        public ReactiveCommand<IElementVM<Event>> SelectEvent { get; private set; }
        #endregion
        
        public ReactiveCollection<EventVM> EventList { get; private set; }              

        public ViewESVM(Container ioc)            
        {
            Storage = ioc.Resolve<IFieldDataService>();

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
                    .ToObservable(Scheduler.ThreadPool)
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
                .Select(_ => Current)
                .ToMessage(MessageContracts.MAPS);
        }       
    }
}
