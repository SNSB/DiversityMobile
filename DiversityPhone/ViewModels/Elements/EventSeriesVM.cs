namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using DiversityPhone.Model;

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

        public EventSeriesVM( EventSeries model )
            : base(model)
        {           
            if (Model.IsNoEventSeries()) //Überprüfen auf NoEventSeries
                _esIcon = ViewModels.Icon.NoEventSeries;
            else
            {
                _esIcon = ViewModels.Icon.EventSeries;                
            }

            Model.ObservableForProperty(x => x.Description)
                .Subscribe(_ => this.RaisePropertyChanged(x => x.Description));
        }        
    }
}




