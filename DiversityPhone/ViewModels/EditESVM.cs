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

    public class EditESVM : ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;        
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Cancel { get; private set; }
        #endregion

        #region Properties
        private EventSeries _Model;
        public EventSeries Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value); }
        }

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

        public DateTime _SeriesEnd;

        public DateTime? SeriesEnd
        {
            get { return _SeriesEnd; }
            set
            {
                if (value != null)
                {
                    if (value >= Model.SeriesStart)
                        this.RaiseAndSetIfChanged(x => x.SeriesEnd, value);
                    else
                    {
                        _messenger.SendMessage<DialogMessage>("The Series has to end after it begins!");
                        this.RaisePropertyChanged(x => x.SeriesEnd);
                    }
                }
            }
        }
        #endregion


        public EditESVM(IMessageBus messenger)
        {

            _messenger = messenger;



            var canSave = this.ObservableForProperty(x => x.Description)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),

                (Cancel = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Message>(Message.NavigateBack)),

                _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                    .Subscribe(es => updateView(es))
            };
        }



        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<EventSeries>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void updateModel()
        {
            Model.Description = Description;
            Model.SeriesCode = SeriesCode;
            Model.SeriesEnd = SeriesEnd ?? Model.SeriesEnd;
        }

        private void updateView(EventSeries es)
        {
            Model = es;
            Description = Model.Description ?? "";
            SeriesCode = Model.SeriesCode;
            SeriesEnd = Model.SeriesEnd;            
        }
    }
}
