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
                return (!Model.IsObservation()) ? Model.AccessionNumber : "Observation";
            }
        }
        public override Icon Icon
        {
            get
            {
                return (Model.IsObservation()) ? ViewModels.Icon.Observation : ViewModels.Icon.Specimen;
            }
        }
        

        public SpecimenVM(Specimen model)
            : base( model)
        {
            if(canSelectPredicate != null)
                CanSelect.OnNext(canSelectPredicate(Model));
        }
    }
}
