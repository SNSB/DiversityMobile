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

    public class ViewCSVM : PageViewModel
    {
        IList<IDisposable> _subscriptions;

        #region Services
        IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand AddIdentificationUnit { get; private set; }
        #endregion

        #region Properties       
        public SpecimenVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<SpecimenVM> _Current;       

        public IList<IdentificationUnitVM> UnitList { get { return _UnitList.Value; } }
        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _UnitList;
        #endregion



        public ViewCSVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var rawModel = StateObservable
                .Select(s => SpecimenFromContext(s.Context));
            var modelDeleted = rawModel.Select(spec => spec == null);
            var validModel = rawModel.Where(spec => spec != null);

            _messenger.RegisterMessageSource(modelDeleted.Select(_ => Message.NavigateBack));

            _Current = validModel.Select(cs => new SpecimenVM(_messenger, cs))
                                .ToProperty(this, x => x.Current);


            _UnitList = validModel
                .Select(cs => getIdentificationUnitList(cs))
                .ToProperty(this, x => x.UnitList);

            _subscriptions = new List<IDisposable>()
            {

            };
        }

        private IList<IdentificationUnitVM> getIdentificationUnitList(Specimen spec)
        {
            return IdentificationUnitVM.getTwoLevelVMFromModelList(
                 _storage.getTopLevelIUForSpecimen(spec),
                 iu => _storage.getSubUnits(iu),
                 _messenger);
        }

        private Specimen SpecimenFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getSpecimenByID(id);
                }
            }            
            return null;
        }

    }
}
