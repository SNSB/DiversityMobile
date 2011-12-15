namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using System.Reactive.Linq;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
    using DiversityPhone.Services;

    public class IdentificationUnitVM : ElementVMBase<IdentificationUnit>
    {        
        public override string Description { get { return string.Format("[{0}] {1}", Model.UnitID, Model.LastIdentificationCache ?? ""); } }
        public override Icon Icon { get { return Icon.IdentificationUnit; } }

        public IList<IdentificationUnitVM> SubUnits { get; private set; }
        public bool HasSubUnits { get { return (SubUnits != null) ? SubUnits.Count > 0 : false; } }


        public IdentificationUnitVM(IMessageBus messenger, IdentificationUnit model, IList<IdentificationUnitVM> subunits)
            : base(messenger, model)
        {
            if (messenger == null) throw new ArgumentNullException("messenger");
            if (model == null) throw new ArgumentNullException("model");
            
            SubUnits = subunits;       

            Select = new ReactiveCommand();
            Edit = new ReactiveCommand();

            Messenger.RegisterMessageSource(
                Select
                .Select(_ => new NavigationMessage(Page.ViewIU, Model.UnitID.ToString()))
                );
                        
            Messenger.RegisterMessageSource(
                Edit
                .Select(_ => new NavigationMessage(Page.EditIU, Model.UnitID.ToString()))
                );
            
        }

        public static IList<IdentificationUnitVM> getTwoLevelVMFromModelList(IList<IdentificationUnit> source, Func<IdentificationUnit, IList<IdentificationUnit>> getSubUnits, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(
                    messenger,
                    iu,
                    getSingleLevelVMFromModelList(getSubUnits(iu), messenger)
                    ));
        }
        public static IList<IdentificationUnitVM> getSingleLevelVMFromModelList(IList<IdentificationUnit> source, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(messenger, iu, null));
        }

        private static IList<IdentificationUnitVM> getVMListFromModelAndFactory(IList<IdentificationUnit> source, Func<IdentificationUnit, IdentificationUnitVM> vmFactory)
        {
            return new VirtualizingReadonlyViewModelList<IdentificationUnit, IdentificationUnitVM>(
                        source,
                        vmFactory
                        );
        }
    }

}
