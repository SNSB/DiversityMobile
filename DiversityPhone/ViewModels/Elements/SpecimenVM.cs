namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using DiversityPhone.Model;

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
            model.ObservableForProperty(x => x.AccessionNumber)
                .Subscribe(_ => this.RaisePropertyChanged(x => x.Description));
        }
    }
}
