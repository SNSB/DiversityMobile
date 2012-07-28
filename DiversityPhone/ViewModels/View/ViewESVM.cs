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

    public class ViewESVM : ElementPageViewModel<EventSeries>
    { 
        #region Commands
        public ReactiveCommand AddEvent { get; private set; }        
        public ReactiveCommand Maps { get; private set; }
        #endregion

        #region Properties
        public ReactiveCollection<EventVM> EventList { get; private set; }        
        #endregion

        private ReactiveAsyncCommand getEvents = new ReactiveAsyncCommand();
        private SerialDisposable model_select = new SerialDisposable();
        private ISubject<IElementVM<Event>> event_selected = new Subject<IElementVM<Event>>();

        public ViewESVM()            
        {   
            EventList = getEvents.RegisterAsyncFunction(es =>
                {
                    return Storage.getEventsForSeries(es as EventSeries)
                        .Select(ev => new EventVM(ev));
                })
                .Do(_ => EventList.Clear())
                .SelectMany(evs => evs)
                .Do(vm => vm.SelectObservable.Subscribe(event_selected.OnNext))
                .CreateCollection();

            ValidModel.Subscribe(getEvents.Execute);

            event_selected
                .Select(vm => vm.Model.EventID.ToString())
                .ToNavigation(Page.ViewEV);

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
            Maps = new ReactiveCommand();
            var mapMessageSource =
                Maps
                .Select(_ => new NavigationMessage(Page.LoadedMaps, null, ReferrerType.EventSeries, Current.Model.SeriesID.ToString()));
            Messenger.RegisterMessageSource(mapMessageSource);           
        }

        protected override EventSeries ModelFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return Storage.getEventSeriesByID(id);
                }
                else
                    return null;
            }
            else
                return EventSeries.NoEventSeries;

        }

        protected override ElementVMBase<EventSeries> ViewModelFromModel(EventSeries model)
        {
            var res = new EventSeriesVM(model);
            if (!EventSeries.isNoEventSeries(model))
            {
                model_select.Disposable = res.SelectObservable
                    .Select(vm => vm.Model.SeriesID.ToString())
                    .ToNavigation(Page.EditES);
            }
            else
                model_select.Disposable = null;
            return res;
        }
    }
}
