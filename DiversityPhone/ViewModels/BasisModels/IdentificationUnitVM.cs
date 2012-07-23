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
        private const int MAX_DISPLAY_DEPTH = 2;

        public override string Description { get { return Model.WorkingName; } }
        public override Icon Icon { get { return Icon.IdentificationUnit; } }

        public IdentificationUnitVM Parent { get; private set; }

        public ReactiveCollection<IdentificationUnitVM> SubUnits { get; private set; } 
        
        public IdentificationUnitVM (IdentificationUnit model, IdentificationUnitVM parent = null)
            : base(model)
	    {
            model.ObservableForProperty(iu => iu.WorkingName)
                .Subscribe(_=>this.RaisePropertyChanged(x => x.Description));       
     
            if(parent != null)
                this.SelectSubject.Subscribe(parent.SelectSubject);
	    }
    }

}
