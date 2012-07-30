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
        private IFieldDataService Storage;

        #region Commands
        public ReactiveCommand AddEvent { get; private set; }        
        public ReactiveCommand Maps { get; private set; }
        #endregion

        #region Properties
        public ReactiveCollection<EventVM> EventList { get; private set; }        
        #endregion

        private ReactiveAsyncCommand getEvents = new ReactiveAsyncCommand();
        private SerialDisposable model_select = new SerialDisposable();
        private ISubject<IElementVM<Event>> select_event = new Subject<IElementVM<Event>>();

        public ViewESVM(Container ioc)            
        {
            Storage = ioc.Resolve<IFieldDataService>();            

            EventList = getEvents.RegisterAsyncFunction(es =>
                {
                    return Storage.getEventsForSeries(es as EventSeries)
                        .Select(ev => new EventVM(ev));
                })
                .Do(_ => EventList.Clear())
                .SelectMany(evs => evs)
                .Do(vm => vm.SelectObservable.Subscribe(select_event.OnNext))
                .CreateCollection();

            CurrentModelObservable.Subscribe(getEvents.Execute);

            select_event
                .Select(vm => vm.Model.EventID.ToString())
                .ToNavigation(Page.ViewEV);

            //On each Invocation of AddEvent, a new NavigationMessage is generated
            AddEvent = new ReactiveCommand();
            var newEventMessageSource =
                AddEvent
                .Timestamp()
                .CombineLatest(CurrentModelObservable, (a, b) => new { Click = a, Model = b })
                .DistinctUntilChanged(pair => pair.Click.Timestamp)
                .Select(pair => new NavigationMessage(Page.EditEV, null, ReferrerType.EventSeries,
                              (EventSeries.isNoEventSeries(pair.Model)) ? null : pair.Model.SeriesID.ToString()
                    ));
            Messenger.RegisterMessageSource(newEventMessageSource);
            Maps = new ReactiveCommand();
            var mapMessageSource =
                Maps
                .Select(_ => new NavigationMessage(Page.LoadedMaps, null, ReferrerType.EventSeries, Current.Model.SeriesID.ToString()));
            Messenger.RegisterMessageSource(mapMessageSource);           
        }       
    }
}
