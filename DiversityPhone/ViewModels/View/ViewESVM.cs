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

            EventList = getEvents.RegisterAsyncFunction(es =>
                {
                    return Storage.getEventsForSeries(es as EventSeries)
                        .Select(ev => new EventVM(ev));
                })                
                .SelectMany(evs => evs)                
                .CreateCollection();

            EventList
                .ListenToChanges<Event, EventVM>(ev => ev.SeriesID == Current.Model.SeriesID);

            CurrentModelObservable
                .Do(_ => EventList.Clear())
                .Subscribe(getEvents.Execute);

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

            Maps = new ReactiveCommand();            
            Maps
                .Select(_ => Current)
                .ToMessage(MessageContracts.MAPS);
        }       
    }
}
