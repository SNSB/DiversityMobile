namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using System.Reactive.Linq;
    using DiversityPhone.Messages;
    using System.Collections.Generic;
    using DiversityPhone.Services;

    public class EventSeriesVM : ElementVMBase<EventSeries>
    {
        public override string Description { get { return Model.Description; } }

        private Icon _esIcon;
        public override Icon Icon
        {
            get
            {
                return _esIcon;
            }
            
        }

        public EventSeriesVM( IMessageBus messenger, EventSeries model, Page targetPage, Predicate<EventSeries> canSelectPredicate = null)
            : base(messenger,model, targetPage)
        {           
            if (EventSeries.isNoEventSeries(Model)) //Überprüfen auf NoEventSeries
                _esIcon = ViewModels.Icon.NoEventSeries;
            else
            {
                _esIcon = ViewModels.Icon.EventSeries;                
            }

            if(canSelectPredicate != null)
                CanSelect.OnNext(canSelectPredicate(Model));
        }

        protected override NavigationMessage NavigationMessage
        {
            get
            {
                return new NavigationMessage(
                    TargetPage,
                    (EventSeries.isNoEventSeries(Model)) ? null : Model.SeriesID.ToString()
                    );
            }
        }
    }
}




