using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Linq;
using System.Reactive.Subjects;
using Funq;
using System.Reactive.Disposables;
namespace DiversityPhone.ViewModels
{
   

    public class ViewCSVM : ViewPageVMBase<Specimen>
    {
        private ReactiveAsyncCommand getSubunits = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getMultimedia = new ReactiveAsyncCommand();

        private IFieldDataService Storage;

        public enum Pivots
        {
            Units,
            Multimedia
        }
     
        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }

        public ReactiveCommand<IElementVM<Specimen>> EditSpecimen { get; private set; }
        public ReactiveCommand<IElementVM<IdentificationUnit>> SelectUnit { get; private set; }
        
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


        public ReactiveCollection<IdentificationUnitVM> UnitList { get; private set; }
        #endregion
       
        public ViewCSVM(Container ioc)             
        {
            Storage = ioc.Resolve<IFieldDataService>();

            EditSpecimen = new ReactiveCommand<IElementVM<Specimen>>(vm => !vm.Model.IsObservation());
            EditSpecimen
                .ToMessage(MessageContracts.EDIT);

            //SubUnits
            UnitList = getSubunits.RegisterAsyncFunction(spec => Storage.getIUForSpecimen((spec as Specimen).SpecimenID).Select(s => new IdentificationUnitVM(s)))
                .SelectMany(vms => vms)
                .CreateCollection();

            UnitList.ListenToChanges<IdentificationUnit, IdentificationUnitVM>(iu => iu.RelatedUnitID == null);

            CurrentModelObservable
                .Do(_ => UnitList.Clear())
                .Subscribe(getSubunits.Execute);

            SelectUnit = new ReactiveCommand<IElementVM<IdentificationUnit>>();
            SelectUnit
                .ToMessage(MessageContracts.VIEW);



            Add = new ReactiveCommand();
            Add.Where(_ => SelectedPivot == Pivots.Units)
                .Select(_ => new IdentificationUnitVM(new IdentificationUnit(){SpecimenID = Current.Model.SpecimenID}) as IElementVM<IdentificationUnit>)
                .ToMessage(MessageContracts.EDIT);
               
               
            Maps = new ReactiveCommand();
            var mapMessageSource =
                Maps
                .Select(_ => new NavigationMessage(Page.LoadedMaps, null, ReferrerType.Specimen, Current.Model.DiversityCollectionSpecimenID.ToString()));
            Messenger.RegisterMessageSource(mapMessageSource);
        }
    }
}
