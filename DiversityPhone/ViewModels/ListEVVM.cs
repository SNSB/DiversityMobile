using System;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Collections.Generic;
using DiversityService.Model;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class ListEVVM : ReactiveObject
    {
        IMessageBus _messenger;
        IOfflineStorage _storage;     

           

        public ReactiveCommand AddEvent { get; private set; }
        public ReactiveCommand FilterEvents { get; private set; }

        public IList<EventVM> _EventList; //Necessary for SL
        public IList<EventVM> EventList
        {
            get
            {
                return _EventList;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.EventList, value);
            }
        }


        public ListEVVM(IMessageBus messenger, Services.IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            (AddEvent = new ReactiveCommand())
                .Subscribe(_ => addEvent());

            FilterEvents = new ReactiveCommand();

            updateList();
        }    

        private void updateList()
        {
            EventList = new VirtualizingReadonlyViewModelList<Event, EventVM>(
                _storage.getAllEvents(),
                (model) => new EventVM(model, _messenger)
                );
        }

        private void saveEvent(Event ev)
        {
            _storage.addEvent(ev);
            updateList();
        }

        private void addEvent()
        {
            _messenger.SendMessage<Event>(
                new Event(),
                MessageContracts.EDIT
                );
            _messenger.SendMessage<Page>(Page.EditEvent);            
        }
    }    
}
