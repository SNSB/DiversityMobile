namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
using DiversityPhone.Services;

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

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.CollectionSpecimenID.ToString()); }
        }

        public SpecimenVM(IMessageBus _messenger, Specimen model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
