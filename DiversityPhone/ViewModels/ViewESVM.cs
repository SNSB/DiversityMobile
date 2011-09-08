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
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class ViewESVM : ReactiveObject
    {
        IMessageBus _messenger;
        IOfflineStorage _storage;
        IList<IDisposable> _subscriptions;

           

        public ReactiveCommand AddEvent { get; private set; }
        public ReactiveCommand FilterEvents { get; private set; }

        
        public EventSeriesVM CurrentSeries { get { return _CurrentSeries.Value; } }
        private ObservableAsPropertyHelper<EventSeriesVM> _CurrentSeries;



        public IList<EventVM> EventList { get { return _EventList.Value; } }
        private ObservableAsPropertyHelper<IList<EventVM>> _EventList;
        


        public ViewESVM(IMessageBus messenger, Services.IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var selectES = _messenger.Listen<EventSeries>(MessageContracts.SELECT);

            _CurrentSeries = selectES
                .Select(es => new EventSeriesVM(es,_messenger))
                .ToProperty(this, x=>x.CurrentSeries);

            _EventList = selectES
                .Select(es => new VirtualizingReadonlyViewModelList<Event, EventVM>(
                    _storage.getEventsForSeries(es),
                    (model) => new EventVM(model, _messenger)
                ) as IList<EventVM>)
                .ToProperty(this, x => x.EventList);

            _subscriptions = new List<IDisposable>()
            {
               
                (AddEvent = new ReactiveCommand())
                    .Subscribe(_ => addEvent()),
            };

            FilterEvents = new ReactiveCommand();

            
        } 
        

        private void saveEvent(Event ev)
        {
            _storage.addEvent(ev);
            
        }

        private void addEvent()
        {
            _messenger.SendMessage<Event>(
                new Event()
                {
                    SeriesID = CurrentSeries.Model.SeriesID
                },
                MessageContracts.EDIT
                );
            _messenger.SendMessage<Page>(Page.EditEvent);            
        }
    }    
}
