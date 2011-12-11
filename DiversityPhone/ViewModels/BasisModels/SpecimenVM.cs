namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;

    public class SpecimenVM : ElementVMBase<Specimen>
    {        
        public override string Description { get { return string.Format("[{0}] {1}", Model.CollectionSpecimenID, Model.AccesionNumber ?? ""); } }
        public override Icon Icon { get { return ViewModels.Icon.Specimen; } }

        public SpecimenVM(IMessageBus messenger, Specimen model)
            : base(messenger,model)
        {
            if (messenger == null) throw new ArgumentNullException("messenger");
            if (model == null) throw new ArgumentNullException("model");
            
            Select = new ReactiveCommand();
            Edit = new ReactiveCommand();

            Messenger.RegisterMessageSource(
                Select
                .Select(_ => new NavigationMessage( Services.Page.ViewCS, Model.CollectionSpecimenID.ToString()))
                );
            Messenger.RegisterMessageSource(
                Edit
                .Select(_ => new NavigationMessage(Services.Page.EditCS, Model.CollectionSpecimenID.ToString()))
                );
        }


        public static IList<SpecimenVM> getSingleLevelVMFromModelList(IList<Specimen> source, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                spec => new SpecimenVM(messenger, spec));
        }

        private static IList<SpecimenVM> getVMListFromModelAndFactory(IList<Specimen> source, Func<Specimen, SpecimenVM> vmFactory)
        {
            return new VirtualizingReadonlyViewModelList<Specimen, SpecimenVM>(
                        source,
                        vmFactory
                        );
        }
    }
}
