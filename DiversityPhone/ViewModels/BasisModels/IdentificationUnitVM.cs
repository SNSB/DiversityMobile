using System;
using ReactiveUI;
using System.Collections.Generic;
using DiversityPhone.Model;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using System.Reactive.Subjects;
using System.Linq;
using Funq;
using System.Threading.Tasks;


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


        
        public IdentificationUnitVM (IdentificationUnit model, IdentificationUnitVM parent = null)
            : base(model)
	    {
            model.ObservableForProperty(iu => iu.WorkingName)
                .Subscribe(_=>this.RaisePropertyChanged(x => x.Description));  

            SubUnits = new ReactiveCollection<IdentificationUnitVM>();
            SubUnits
                .ObserveCollectionChanged()
                .Select(_ => SubUnits.Any())
                .BindTo(this, x => x.HasSubUnits);
	    }
    }

}
