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

        public EventSeriesVM( IMessageBus messenger,EventSeries model)
            : base(messenger,model)
        {           
            if (EventSeries.isNoEventSeries(Model)) //Überprüfen auf NoEventSeries
                _esIcon = ViewModels.Icon.NoEventSeries;
            else
            {
                _esIcon = ViewModels.Icon.EventSeries;                
            }

            Select = new ReactiveCommand();
            Edit = new ReactiveCommand();

            Messenger.RegisterMessageSource(
                Select
                .Select(_ => 
                    new NavigationMessage(Page.ViewES,
                        (EventSeries.isNoEventSeries(Model)) ? null : Model.SeriesID.ToString()
                        )));
            Messenger.RegisterMessageSource(
                Edit
                .Where(_ => !EventSeries.isNoEventSeries(Model)) //NoEventSeries nicht editierbar
                .Select(_ => new NavigationMessage(Page.EditES, Model.SeriesID.ToString()))
                );
                            
        }
    }
}




