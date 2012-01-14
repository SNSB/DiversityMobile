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
        public override string Description { get { return string.Format("[{0}] {1}", Model.UnitID, Model.WorkingName ?? ""); } }
        public override Icon Icon { get { return Icon.IdentificationUnit; } }

        public IList<IdentificationUnitVM> SubUnits { get; private set; }
        public bool HasSubUnits { get { return (SubUnits != null) ? SubUnits.Count > 0 : false; } }

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.UnitID.ToString()); }
        }

        public IdentificationUnitVM(IMessageBus messenger, IdentificationUnit model, Page targetPage, IList<IdentificationUnitVM> subunits)
            : base(messenger, model,targetPage)
        {          
            SubUnits = subunits;    
        }

        public static IList<IdentificationUnitVM> getTwoLevelVMFromModelList(IList<IdentificationUnit> source, Func<IdentificationUnit, IList<IdentificationUnit>> getSubUnits, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(
                    messenger,
                    iu,
                    Page.ViewIU,
                    getSingleLevelVMFromModelList(getSubUnits(iu), messenger)
                    ));
        }
        public static IList<IdentificationUnitVM> getSingleLevelVMFromModelList(IList<IdentificationUnit> source, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(messenger, iu, Page.ViewIU, null));
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
