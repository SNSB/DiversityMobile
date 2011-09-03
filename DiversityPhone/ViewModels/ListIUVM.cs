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
    public class ListIUVM : ReactiveObject
    {
        IMessageBus _messenger;
        IOfflineStorage _storage;
        IList<IDisposable> _subscriptions;


        public EventVM CurrentEvent { get { return _CurrentEvent.Value; } }
        private ObservableAsPropertyHelper<EventVM> _CurrentEvent;
        

        public IList<IdentificationUnitVM> UnitList { get { return _UnitList.Value; } }
        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _UnitList;

        public ReactiveCommand Add { get; private set; }
        

        public ListIUVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var eventSelected = _messenger.Listen<Event>(MessageContracts.SELECT);
            _CurrentEvent = eventSelected.Select(ev => new EventVM(ev, _messenger))
                                .ToProperty(this, x => x.CurrentEvent);

            _UnitList = eventSelected.Select(ev =>
                 new VirtualizingReadonlyViewModelList<IdentificationUnit, IdentificationUnitVM>(
                        _storage.getIUForEvent(ev),
                        iu => new IdentificationUnitVM(_messenger, iu)
                        ) as IList<IdentificationUnitVM>
                ).ToProperty(this, x => x.UnitList);
                


            _subscriptions = new List<IDisposable>()
            {
                (Add = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnit>(
                        new IdentificationUnit()
                        {
                            EventID = CurrentEvent.Model.EventID
                        },
                        MessageContracts.EDIT))
                        

            };
        }
    }
}
