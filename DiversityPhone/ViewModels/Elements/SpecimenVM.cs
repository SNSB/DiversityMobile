namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;

    public class SpecimenVM : ElementVMBase<Specimen>
    {
        public override string Description
        {
            get
            {
                return (!Model.IsObservation()) ? Model.AccessionNumber : DiversityResources.Specimen_Observation;
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
            : base(model)
        {
            model.ObservableForProperty(x => x.AccessionNumber)
                .Subscribe(_ => this.RaisePropertyChanged(x => x.Description));
        }
    }
}