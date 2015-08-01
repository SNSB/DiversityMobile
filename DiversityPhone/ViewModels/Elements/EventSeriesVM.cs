namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;

    public class EventSeriesVM : ElementVMBase<EventSeries>
    {
        public override string Description { get { return Model.Description; } }

        public override string Subtitle { get { return Model.SeriesCode; } }

        private Icon _esIcon;

        public override Icon Icon
        {
            get
            {
                return _esIcon;
            }
        }

        public EventSeriesVM(EventSeries model)
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
            Model.ObservableForProperty(x => x.SeriesCode)
                .Subscribe(_ => this.RaisePropertyChanged(x => x.Subtitle));
        }
    }
}