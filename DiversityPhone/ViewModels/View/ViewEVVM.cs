﻿namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using System.Reactive.Linq;
    using DiversityPhone.Services;
    using ReactiveUI.Xaml;

    public class ViewEVVM : PageViewModel
    {
        IList<IDisposable> _subscriptions;

        #region Services
        IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Add { get; private set; }
        #endregion

        #region Properties
        public EventVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<EventVM> _Current;        

        public IList<SpecimenVM> SpecList { get { return _SpecList.Value; } }
        private ObservableAsPropertyHelper<IList<SpecimenVM>> _SpecList;
        #endregion




        public ViewEVVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var eventSelected = _messenger.Listen<Event>(MessageContracts.SELECT);
            var unitSaved = _messenger.Listen<IdentificationUnit>(MessageContracts.SAVE);

            _Current = eventSelected.Select(ev => new EventVM(ev, _messenger))
                                .ToProperty(this, x => x.Current);            

            _SpecList = unitSaved.Select(_ => Current.Model)
                .Merge(eventSelected)
                .Select(ev => getSpecimenList(ev))
                .ToProperty(this, x => x.SpecList);



            _subscriptions = new List<IDisposable>()
            {
                (Add = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnit>(
                        new IdentificationUnit()
                        {
                            SpecimenID = Current.Model.EventID
                        },
                        MessageContracts.EDIT)),                        
                
            };
        }

        private IList<SpecimenVM> getSpecimenList(Event ev)
        {
            return new VirtualizingReadonlyViewModelList<Specimen, SpecimenVM>(
                _storage.getSpecimenForEvent(ev),
                (model) => new SpecimenVM(_messenger, model)
                );
        }
    }
}
