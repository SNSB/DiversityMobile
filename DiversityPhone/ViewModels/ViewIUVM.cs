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
        
        #endregion

        public ViewIUVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;            

            _Current = _messenger
                .Listen<IdentificationUnit>(MessageContracts.SELECT)
                .Where(iu => iu != null)
                .Select(iu => fillIUVM(iu))
            .ToProperty(this, x => x.Current);

            var newSubUnits = (AddSubunit = new ReactiveCommand())
                                .Select(_ => new IdentificationUnit() 
                                { 
                                    SpecimenID = Current.Model.SpecimenID, 
                                    RelatedUnitID = Current.Model.UnitID 
                                });
            _messenger.RegisterMessageSource(newSubUnits, MessageContracts.EDIT);



            _subscriptions = new List<IDisposable>()
            {

            };
        } 
      
        private IdentificationUnitVM fillIUVM(IdentificationUnit iu)
        {
            return new IdentificationUnitVM(
                _messenger,
                iu,
                IdentificationUnitVM.getTwoLevelVMFromModelList(
                    _storage.getSubUnits(iu),
                    unit => _storage.getSubUnits(unit),
                    _messenger));
                    
        }
    }
}
