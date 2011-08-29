using System;
using System.Net;
using System.Reactive.Linq;
using ReactiveUI;
using DiversityService.Model;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class EditESVM : ReactiveObject
    {
        private IMessageBus _messenger;
        private INavigationService _navigation;
        

        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Cancel { get; private set; }
        public ReactiveCommand EditSeriesEnd { get; private set; }

        public EventSeries _Model; //Need to be public in SL :/

        public EventSeries Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, value); }
        }

        public string _Description; //Need to be public in SL :/

        public string Description
        {
            get { return _Description; }
            set { this.RaiseAndSetIfChanged(x=>x.Description,value); }
        }

        public string _SeriesCode; //Need to be public in SL :/

        public string SeriesCode
        {
            get { return _SeriesCode; }
            set { this.RaiseAndSetIfChanged(x => x.SeriesCode, value); }
        }

        public string SeriesStart
        {
            get
            {
                return Model.SeriesStart.ToShortDateString();
            }
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
        

        public EditESVM(INavigationService nav, IMessageBus messenger)
        {
            _navigation = nav;
            _messenger = messenger;

            _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                .Subscribe(es => updateView(es));

            var descriptionObservable = this.ObservableForProperty(x => x.Description);
            var canSave = descriptionObservable.Select(desc => !string.IsNullOrWhiteSpace(desc.Value)).StartWith(false);

            (Save = new ReactiveCommand(canSave))               
                .Subscribe(_ => executeSave());

            (Cancel = new ReactiveCommand())
                .Subscribe(_ => _navigation.NavigateBack());

        }

       

        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<EventSeries>(Model, MessageContracts.SAVE);
            _navigation.NavigateBack();
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
