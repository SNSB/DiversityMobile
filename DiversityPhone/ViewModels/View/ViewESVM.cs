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
        #region Commands
        public ReactiveCommand AddEvent { get; private set; }
        public ReactiveCommand FilterEvents { get; private set; }
        #endregion

        #region Properties
        public IList<EventVM> EventList { get { return _EventList.Value; } }
        private ObservableAsPropertyHelper<IList<EventVM>> _EventList;
        #endregion

        

        public ViewESVM()            
        {   
            _EventList = ValidModel
                .Select(es => new VirtualizingReadonlyViewModelList<Event, EventVM>(
                    Storage.getEventsForSeries(es),
                    (model) => new EventVM(Messenger, model, Page.ViewEV)
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
            return new EventSeriesVM(Messenger, model, Page.EditES);
        }
    }
}
