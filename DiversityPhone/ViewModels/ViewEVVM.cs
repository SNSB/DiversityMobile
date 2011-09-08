using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public class ViewEVVM : ReactiveObject
    {
        IMessageBus _messenger;
        IOfflineStorage _storage;
        IList<IDisposable> _subscriptions;


        public EventVM CurrentEvent { get { return _CurrentEvent.Value; } }
        private ObservableAsPropertyHelper<EventVM> _CurrentEvent;

        
        private Event Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<Event> _Model;
        
        

        public IList<IdentificationUnitVM> UnitList { get { return _UnitList.Value; } }
        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _UnitList;

        public ReactiveCommand Add { get; private set; }
        

        public ViewEVVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var eventSelected = _messenger.Listen<Event>(MessageContracts.SELECT);
            var unitSaved = _messenger.Listen<IdentificationUnit>(MessageContracts.SAVE);

            _CurrentEvent = eventSelected.Select(ev => new EventVM(ev, _messenger))
                                .ToProperty(this, x => x.CurrentEvent);
            _Model = eventSelected.ToProperty(this, x => x.Model);

            _UnitList = unitSaved.Select(_ => Model)
                .Merge(eventSelected)
                .Select(ev => getNewUnitList(ev))
                .ToProperty(this, x => x.UnitList);
                


            _subscriptions = new List<IDisposable>()
            {
                (Add = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnit>(
                        new IdentificationUnit()
                        {
                            EventID = Model.EventID
                        },
                        MessageContracts.EDIT)),                        
                
            };
        }

        private IList<IdentificationUnitVM> getNewUnitList(Event ev)
        {
            return IdentificationUnitVM.getTwoLevelVMFromModelList(
                 _storage.getIUForEvent(ev),
                 iu => _storage.getSubUnits(iu),
                 _messenger);
        }
    }
}
