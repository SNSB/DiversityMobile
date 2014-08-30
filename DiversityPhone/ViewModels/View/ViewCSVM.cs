namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public class ViewCSVM : ViewPageVMBase<Specimen>
    {
        private readonly IFieldDataService Storage;

        private ReactiveAsyncCommand getSubunits = new ReactiveAsyncCommand();

        public enum Pivots
        {
            Units,
            Multimedia
        }

        #region Commands

        public ReactiveCommand Add { get; private set; }

        public ReactiveCommand<IElementVM<Specimen>> EditSpecimen { get; private set; }

        public ReactiveCommand<IElementVM<IdentificationUnit>> SelectUnit { get; private set; }

        #endregion Commands

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

        public ElementMultimediaVM MultimediaList { get; private set; }

        public ReactiveCollection<IdentificationUnitVM> UnitList { get; private set; }

        #endregion Properties

        public ViewCSVM(
            IFieldDataService Storage,
            [Dispatcher] IScheduler Dispatcher,
            ElementMultimediaVM MultimediaList
            )
        {
            this.Storage = Storage;

            EditSpecimen = new ReactiveCommand<IElementVM<Specimen>>(vm => !vm.Model.IsObservation());
            EditSpecimen
                .ToMessage(Messenger, MessageContracts.EDIT);

            //SubUnits
            UnitList = getSubunits.RegisterAsyncFunction(spec => buildIUTree(spec as Specimen))
                .SelectMany(vms => vms)
                .ObserveOn(Dispatcher)
                .CreateCollection();

            UnitList.ListenToChanges<IdentificationUnit, IdentificationUnitVM>(iu => iu.RelatedUnitID == null);

            CurrentModelObservable
                .Do(_ => UnitList.Clear())
                .Subscribe(getSubunits.Execute);

            SelectUnit = new ReactiveCommand<IElementVM<IdentificationUnit>>();
            SelectUnit
                .ToMessage(Messenger, MessageContracts.VIEW);

            //Multimedia
            this.MultimediaList = MultimediaList;
            CurrentModelObservable
                .Select(m => m as IMultimediaOwner)
                .Subscribe(MultimediaList);

            Add = new ReactiveCommand();
            Add.Where(_ => SelectedPivot == Pivots.Units)
                .Select(_ => new IdentificationUnitVM(new IdentificationUnit() { SpecimenID = Current.Model.SpecimenID, RelatedUnitID = null }) as IElementVM<IdentificationUnit>)
                .ToMessage(Messenger, MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Multimedia)
                .Subscribe(MultimediaList.AddMultimedia.Execute);
        }

        private IEnumerable<IdentificationUnitVM> buildIUTree(Specimen spec)
        {
            IDictionary<int, IdentificationUnitVM> vmMap = new Dictionary<int, IdentificationUnitVM>();
            IList<IdentificationUnitVM> toplevel = new List<IdentificationUnitVM>();

            Queue<IdentificationUnit> work_left = new Queue<IdentificationUnit>(Storage.getIUForSpecimen(spec.SpecimenID));

            while (work_left.Any())
            {
                var unit = work_left.Dequeue();
                IdentificationUnitVM vm;

                if (unit.RelatedUnitID.HasValue)
                {
                    IdentificationUnitVM parent;
                    if (vmMap.TryGetValue(unit.RelatedUnitID.Value, out parent))
                    {
                        vm = new IdentificationUnitVM(unit);
                        parent.SubUnits.Add(vm);
                    }
                    else
                    {
                        work_left.Enqueue(unit);
                        continue;
                    }
                }
                else
                {
                    vm = new IdentificationUnitVM(unit);
                    toplevel.Add(vm);
                }

                vmMap.Add(unit.UnitID, vm);
            }

            return toplevel;
        }
    }
}