namespace DiversityPhone.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using ReactiveUI;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
    using System.Collections.Generic;
    using DiversityPhone.Services;
    using System.Data.Linq.Mapping;
    using System.Reflection;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Navigation;

    public class EditESVM : EditElementPageVMBase<EventSeries>
    {
        private ISettingsService _settings;

        public ReactiveCommand FinishSeries { get; private set; }

        #region Properties
        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { this.RaiseAndSetIfChanged(x => x.Description, ref _Description, value); }
        }

        private string _SeriesCode;
        public string SeriesCode
        {
            get { return _SeriesCode; }
            set { this.RaiseAndSetIfChanged(x => x.SeriesCode, ref _SeriesCode, value); }
        }

        public ObservableAsPropertyHelper<string> _SeriesStart;
        public string SeriesStart
        {
            get
            {
                return _SeriesStart.Value;                
            }
        }

        private DateTime? _SeriesEnd;
        public DateTime? SeriesEnd
        {
            get
            {
                return _SeriesEnd;
            }
            set
            {                
                this.RaiseAndSetIfChanged(x => x.SeriesEnd,ref _SeriesEnd, value);
            }
        }
        #endregion


        public EditESVM(ISettingsService settings)
        {
            _settings = settings;
        

            ValidModel
                .Select(es => es.Description ?? String.Empty)
                .BindTo(this, x => x.Description);

            ValidModel
                .Select(es => es.SeriesCode)
                .BindTo(this, x => x.SeriesCode);

            ValidModel
                .Select(es => es.SeriesEnd)
                .BindTo(this, x => x.SeriesEnd);

            _SeriesStart = this.ObservableToProperty(
                ValidModel
                .Select(es => es.SeriesStart)
                .Select(start => String.Format("{0} {1}", start.ToShortDateString(), start.ToShortTimeString())),
                x => x.SeriesStart);

            (FinishSeries = new ReactiveCommand()).Subscribe(_ =>finishSeries());
            FinishSeries
                .Select(_ => DateTime.Now as DateTime?)
                .BindTo(this, x => x.SeriesEnd);

        }        

        //Auf diese Weise muss bei dem Hinzufügen eines Feldes in der Datenbank hier der Code angepasst werden        
        protected override IObservable<bool> CanSave()
        {            
            var descriptionNonEmpty = 
                this.ObservableForProperty(x => x.Description)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);

            var endsAfterItBegins =
                this.ObservableForProperty(x => x.SeriesEnd)
                .CombineLatest(ValidModel, (end, model) => new { SeriesEnd = end, Model = model })
                .Select(pair => (pair.SeriesEnd == null) ? true : pair.SeriesEnd.Value > pair.Model.SeriesStart)
                .StartWith(true);
            
            return descriptionNonEmpty.BooleanAnd(endsAfterItBegins);
        }

        protected override void UpdateModel()
        {
            Current.Model.Description = Description;
            Current.Model.SeriesCode = SeriesCode;
            Current.Model.SeriesEnd = SeriesEnd ?? Current.Model.SeriesEnd;
        }

        protected void finishSeries()
        {
            Messenger.SendMessage<EventSeries>(Current.Model, MessageContracts.STOP);
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
            }
            return new EventSeries();
        }

        protected override ElementVMBase<EventSeries> ViewModelFromModel(EventSeries model)
        {
            return new EventSeriesVM(Messenger, model, Page.Current);
        }
    }
}
