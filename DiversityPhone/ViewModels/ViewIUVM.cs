namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using DiversityPhone.Services;
    using ReactiveUI.Xaml;

    public class ViewIUVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;

        #region Services
        IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand AddSubunit { get; private set; }
        #endregion

        #region Properties
        public IdentificationUnitVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnitVM> _Current;

        //Liste Subunits

        private IdentificationUnit Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnit> _Model;
        #endregion

        public ViewIUVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            _Model = _messenger.Listen<IdentificationUnit>(MessageContracts.SELECT)
                .ToProperty(this, x => x.Model);

            _Current = _Model.Where(m => m != null)
                .Select(m => new IdentificationUnitVM(
                _messenger,
                m,
                IdentificationUnitVM.getTwoLevelVMFromModelList(
                _storage.getSubUnits(m),
                iu => _storage.getSubUnits(iu),
                _messenger)
                )).ToProperty(this, x => x.Current);

            var newSubUnits = (AddSubunit = new ReactiveCommand())
                                .Select(_ => new IdentificationUnit() { SpecimenID = Model.SpecimenID, RelatedUnitID = Model.UnitID });
            _messenger.RegisterMessageSource(newSubUnits, MessageContracts.EDIT);



            _subscriptions = new List<IDisposable>()
            {

            };
        }       
    }
}
