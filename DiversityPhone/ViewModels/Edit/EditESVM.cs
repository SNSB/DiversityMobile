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

    public class EditESVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        private IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties
        public Icon ImageSource { get { return Icon.EventSeries; } }

        public ObservableAsPropertyHelper<bool> _Editable;
        public bool Editable 
        { 
            get 
            { 
                return _Editable.Value; 
            } 
        }

        private ObservableAsPropertyHelper<EventSeries> _Model;
        public EventSeries Model
        {
            get { return _Model.Value; }           
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


        public EditESVM(IMessageBus messenger, IOfflineStorage storage)
        { 


            _messenger = messenger;
            _storage = storage;

            var nullOrModel = StateObservable
                .Select(s => EventSeriesFromContext(s.Context));          

            var model = nullOrModel
                .Where(es => es != null);
            _Model = model
                .ToProperty(this, vm => vm.Model);
            

            var invalidSeries = nullOrModel
                .Where(es => es == null);
            _messenger.RegisterMessageSource(invalidSeries.Select(_ => Message.NavigateBack));

            ToggleEditable = new ReactiveCommand();
            _Editable = StateObservable
                .Select(s => s.Context == null)
                .Merge(
                    ToggleEditable.Select(_ => !Editable)
                )
                .ToProperty(this, vm => vm.Editable);



                    

            
            var canSave = this.ObservableForProperty(x => x.Description)
              .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
              .StartWith(false);

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),
                
                (Delete = new ReactiveCommand())
                    .Subscribe(_ => executeDelete()),

                model.Subscribe( es => updateView(es)),
            
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
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void executeDelete()
        {
            _messenger.SendMessage<EventSeries>(Model, MessageContracts.DELETE);
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
            Description = es.Description ?? "";
            SeriesCode = es.SeriesCode;            
            SeriesEnd = es.SeriesEnd;            
        }

        private EventSeries EventSeriesFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getEventSeriesByID(id);
                }
            }
            return new EventSeries();
        }
    }
}
