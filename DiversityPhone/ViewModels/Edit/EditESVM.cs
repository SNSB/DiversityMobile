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

    public class EditESVM : ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;        
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties
        public Icon ImageSource { get { return Icon.EventSeries; } }

        public bool _editable;
        public bool Editable { get { return _editable; } set { this.RaiseAndSetIfChanged(x => x.Editable, ref _editable, value); } }

        private EventSeries _Model;
        public EventSeries Model
        {
            get { return _Model; }
            set {            
                this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value);
                if (Model != null)
                {
                    if (Model.SeriesID == 0 && !Model.Description.Equals(EventSeries.NoEventSeries().Description))
                        Editable = true;
                    else
                        Editable = false;
                }
            }
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

        public string _SeriesStart;
        public string SeriesStart
        {
            get
            {
                if (Model != null)
                    return this.RaiseAndSetIfChanged(x => x.SeriesStart, String.Concat(Model.SeriesStart.ToShortDateString()," ",Model.SeriesStart.ToShortTimeString()));
                else
                    return String.Empty;
            }

        }

        public DateTime? _SeriesEnd;

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
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => setEdit()),
                (Delete = new ReactiveCommand())
                    .Subscribe(_ => executeDelete()),
                _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                    .Subscribe(es => updateView(es))
            };
        }

        IObservable<bool> canSave(IList<MemberInfo> notNullable)
        {
            IObservable<bool> canSave = this.ObservableForProperty(x => x.Editable)
                    .Select(edit => this.Editable)
                    .StartWith(false);
            foreach (MemberInfo mi in notNullable)
            {
                if (mi.MemberType == MemberTypes.Property)
                {
                    IObservable<bool> attribute = this.ObservableForProperty(x => mi) //Geht nicht mit dem Ausdruck
                        .Select(att => !(att.Value==null))
                        .StartWith(false);
                    canSave = Extensions.BooleanAnd(canSave, attribute);
                }
            }
            return canSave;
        }

        //Auf diese Weise muss bei dem Hinzufügen eines Feldes in der Datenbank hier der Code angepasst werden
        IObservable<bool> canSave()
        {
            IObservable<bool> editable = this.ObservableForProperty(x => x.Editable)
                    .Select(edit => this.Editable)
                    .StartWith(false);
            IObservable<bool> description = this.ObservableForProperty(x => x.Description)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);
            IObservable<bool> canSave = Extensions.BooleanAnd(editable, description);
            return canSave;
        }

        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<EventSeries>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<EventSeries>(Model, MessageContracts.SELECT);
            //EventSeriesVM vm = new EventSeriesVM(Model, _messenger);//Create Model
            
            //_messenger.SendMessage<Message>(Message.NavigateBack);
            _messenger.SendMessage<Page>(Page.ViewES);
        }

        private void executeDelete()
        {
            _messenger.SendMessage<EventSeries>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Page>(Page.Home);
        }


        private void setEdit()
        {
            if (Editable == false)
                Editable = true;
            else
                Editable = false;
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
            string s = SeriesStart;
            SeriesEnd = Model.SeriesEnd;            
        }
    }
}
