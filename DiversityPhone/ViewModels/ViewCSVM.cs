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

    public class ViewCSVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;

        #region Services
        IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        public SpecimenVM CurrentSpecimen { get { return _CurrentSpecimen.Value; } }
        private ObservableAsPropertyHelper<SpecimenVM> _CurrentSpecimen;


        private Specimen Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<Specimen> _Model;

        #region Commands
        public ReactiveCommand AddSubunit { get; private set; }
        #endregion

        #region Properties

        //List<IdentificationUnitVM> Units;
        public IList<IdentificationUnitVM> UnitList { get { return _UnitList.Value; } }
        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _UnitList;
        #endregion



        public ViewCSVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var specSelected = _messenger.Listen<Specimen>(MessageContracts.SELECT);
            var unitSaved = _messenger.Listen<IdentificationUnit>(MessageContracts.SAVE);

            _CurrentSpecimen = specSelected.Select(cs => new SpecimenVM( _messenger,cs))
                                .ToProperty(this, x => x.CurrentSpecimen);
            _Model = specSelected.ToProperty(this, x => x.Model);

            _UnitList = unitSaved.Select(_ => Model)
                .Merge(specSelected)
                .Select(cs => getNewUnitList(cs))
                .ToProperty(this, x => x.UnitList);

            _subscriptions = new List<IDisposable>()
            {

            };
        }

        private IList<IdentificationUnitVM> getNewUnitList(Specimen spec)
        {
            return IdentificationUnitVM.getTwoLevelVMFromModelList(
                 _storage.getTopLevelIUForSpecimen(spec),
                 iu => _storage.getSubUnits(iu),
                 _messenger);
        }

    }
}
