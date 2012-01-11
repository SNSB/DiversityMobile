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
        public override string Description
        {
            get
            {
                return Model.AccessionNumber ?? "Observation";
            }
        }
        public override Icon Icon
        {
            get
            {
                return (Model.AccessionNumber == null) ? ViewModels.Icon.Observation : ViewModels.Icon.Specimen;
            }
        }

        public SpecimenVM(IMessageBus messenger, Specimen model)
            : base(messenger, model)
        {
            if (messenger == null) throw new ArgumentNullException("messenger");
            if (model == null) throw new ArgumentNullException("model");

            Select = new ReactiveCommand();
            Edit = new ReactiveCommand();

            Messenger.RegisterMessageSource(
                Select
                .Select(_ => new NavigationMessage(Services.Page.ViewCS, Model.CollectionSpecimenID.ToString()))
                );
            Messenger.RegisterMessageSource(
                Edit
                .Select(_ => new NavigationMessage(Services.Page.EditCS, Model.CollectionSpecimenID.ToString()))
                );
        }
    }
}
