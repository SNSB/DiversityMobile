using System;
using ReactiveUI;
using DiversityPhone.Model;
using System.Reactive.Linq;


namespace DiversityPhone.ViewModels
{
   
    public class IdentificationUnitVM : ElementVMBase<IdentificationUnit>
    {   
        public override string Description { get { return Model.WorkingName; } }
        public override Icon Icon { get { return Icon.IdentificationUnit; } }        

        public ReactiveCollection<IdentificationUnitVM> SubUnits { get; private set; }

        private bool _HasSubUnits;

        public bool HasSubUnits
        {
            get { return _HasSubUnits; }
            set { this.RaiseAndSetIfChanged(x => x.HasSubUnits, ref _HasSubUnits, value); }
        }


        
        public IdentificationUnitVM (IdentificationUnit model)
            : base(model)
	    {
            model.ObservableForProperty(iu => iu.WorkingName)
                .Subscribe(_=>this.RaisePropertyChanged(x => x.Description));  

            SubUnits = new ReactiveCollection<IdentificationUnitVM>();
            SubUnits
                .CollectionCountChanged
                .Select(c => c > 0)
                .Subscribe(x => HasSubUnits = x);
	    }
    }

}
