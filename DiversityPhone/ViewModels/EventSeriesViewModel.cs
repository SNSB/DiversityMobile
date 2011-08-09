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
    public class EventSeriesViewModel : ReactiveObject
    {
        private INavigationService _navigation;
        private IOfflineStorage _storage;
        private IMessageBus _messenger;

        public ReactiveCommand AddSeries { get; private set; }

        
        public EventSeriesViewModel(INavigationService nav, IOfflineStorage storage, IMessageBus messenger)       
        {
            _navigation = nav;
            _storage = storage;
            _messenger = messenger;


            (AddSeries = new ReactiveCommand())
                .Subscribe(_ => addSeries());


        }

        private void addSeries()
        {
            _messenger.SendMessage<EventSeries>(
                new EventSeries(),
                MessageContracts.EDIT
                );
            _navigation.Navigate(Page.EditEventSeries);
            
        }
    }
}
