using System;
using System.Net;
using ReactiveUI;
using System.Collections;
using System.Collections.Generic;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class ListESVM : ReactiveObject
    {
        private INavigationService _navigation;
        private IOfflineStorage _storage;
        private IMessageBus _messenger;

        public ReactiveCommand AddSeries { get; private set; }
        public ReactiveCommand FilterSeries { get; private set; }

        public IList<EventSeriesVM> _SeriesList; //Necessary for SL
        public IList<EventSeriesVM> SeriesList
        {
            get
            {
                return _SeriesList;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.SeriesList, value);
            }
        }       

        
        public ListESVM(INavigationService nav, IOfflineStorage storage, IMessageBus messenger)       
        {
            _navigation = nav;
            _storage = storage;
            _messenger = messenger;

            _messenger.Listen<EventSeries>(MessageContracts.SAVE)
                .Subscribe(es => saveSeries(es));

            _messenger.Listen<EventSeries>(MessageContracts.SELECT)
                .Subscribe(es => selectSeries(es));

            updateSeriesList();


            (AddSeries = new ReactiveCommand())
                .Subscribe(_ => addSeries());


            FilterSeries = new ReactiveCommand();
        }

        private void selectSeries(EventSeries es)
        {
            _navigation.Navigate(Page.ViewEventSeries);
        }

        private void updateSeriesList()
        {
            SeriesList = new VirtualizingReadonlyViewModelList<EventSeries, EventSeriesVM>(
                _storage.getAllEventSeries(),
                (model) => new EventSeriesVM(model, _messenger)
                );
        }

        private void saveSeries(EventSeries es)
        {
            _storage.addEventSeries(es);
            updateSeriesList();
        }

        private void addSeries()
        {
            _messenger.SendMessage<EventSeries>(
                new EventSeries(),
                MessageContracts.EDIT
                );            
        }
    }
}
