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
        public enum Pivots
        {
            Units,
            Multimedia
        }       

        #region Services
        IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Add { get; private set; }
        #endregion

        #region Properties       

        private Pivots _SelectedPivot;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedPivot, ref _SelectedPivot, value);
            }
        }
        

        public SpecimenVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<SpecimenVM> _Current;       

        public IList<IdentificationUnitVM> UnitList { get { return _UnitList.Value; } }
        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _UnitList;
        #endregion



        public ViewCSVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            Add = new ReactiveCommand();

            var rawModel = StateObservable
                .Select(s => SpecimenFromContext(s.Context));
            var modelDeleted = rawModel.Where(spec => spec == null);
            var validModel = rawModel.Where(spec => spec != null);

            _messenger.RegisterMessageSource(modelDeleted.Select(_ => Message.NavigateBack));

            _Current = validModel.Select(cs => new SpecimenVM(_messenger, cs))
                                .ToProperty(this, x => x.Current);


            _UnitList = validModel
                .Select(cs => getIdentificationUnitList(cs))
                .ToProperty(this, x => x.UnitList);

            _messenger.RegisterMessageSource(
                Add
                .Select(_ =>
                {
                    switch (SelectedPivot)
                    {
                        case Pivots.Multimedia:
                            return Page.EditMMO;
                        case Pivots.Units:
                        default:
                            return Page.EditIU;
                    }
                })
                .Select(p => new NavigationMessage(p, null, ReferrerType.Specimen, Current.Model.CollectionSpecimenID.ToString()))
                );
            
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
