namespace DiversityPhone.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using ReactiveUI;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    
    using DiversityPhone.Interface;

    public class EditESVM : EditPageVMBase<EventSeries>
    {
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


        public EditESVM()
        {

            ModelByVisitObservable
                .Select(es => es.Description ?? String.Empty)
                .Subscribe(x => Description = x);

            ModelByVisitObservable
                .Select(es => es.SeriesCode)
                .Subscribe(x => SeriesCode = x);

            ModelByVisitObservable
                .Select(es => es.SeriesEnd)
                .Subscribe(x => SeriesEnd = x);

            _SeriesStart = this.ObservableToProperty(
                CurrentModelObservable
                .Select(es => es.SeriesStart)
                .Select(start => String.Format("{0} {1}", start.ToShortDateString(), start.ToShortTimeString())),
                x => x.SeriesStart);

            (FinishSeries = new ReactiveCommand(CurrentModelObservable.Select(es => es.SeriesEnd == null)))            
                .Select(_ => DateTime.Now as DateTime?)
                .Subscribe(x => SeriesEnd = x);

            Save
                .Where(_ => _SeriesEnd != null)
                .Subscribe(_ => Messenger.SendMessage<EventSeries>(null, MessageContracts.STOP));

                 
        }        

        //Auf diese Weise muss bei dem Hinzufügen eines Feldes in der Datenbank hier der Code angepasst werden        
        private IObservable<bool> CanSave()
        {            
            var descriptionNonEmpty = 
                this.WhenAny(x => x.Description, x => x.Value)
                .Select(desc => !string.IsNullOrWhiteSpace(desc))
                .StartWith(false);

            var endsAfterItBegins =
                this.WhenAny(x => x.SeriesEnd, x => x.Value)
                .CombineLatest(CurrentModelObservable, (end, model) => new { SeriesEnd = end, Model = model })
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

             
    }
}
