using System;
using System.Net;
using ReactiveUI;
using System.Collections;
using System.Collections.Generic;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using DiversityService.Model;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class ListESVM : ReactiveObject
    {
        private INavigationService _navigation;
        private IOfflineStorage _storage;
        private IMessageBus _messenger;

        public ReactiveCommand AddSeries { get; private set; }

        public IList<EventSeries> _SeriesList; //Necessary for SL
        public IList<EventSeries> SeriesList
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

            updateSeriesList();


            (AddSeries = new ReactiveCommand())
                .Subscribe(_ => addSeries());


        }

        private void updateSeriesList()
        {
            SeriesList = _storage.getAllEventSeries();
        }

        private void saveSeries(EventSeries es)
        {
            _storage.addEventSeries(es);
            updateSeriesList();
        }

        private void addSeries()
        {
            _messenger.SendMessage<EventSeries>(
                new EventSeries() { Description = "Test" },
                MessageContracts.EDIT
                );
            _navigation.Navigate(Page.EditEventSeries);
            
        }
    }
}
