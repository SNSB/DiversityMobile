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

        public ReactiveCollection<IdentificationUnitVM> SubUnits { get; private set; }

        public ReactiveCollection<IdentificationUnit> UnitPool { get; private set; }

        private int _Level;

        public int Level
        {
            get
            {
                return _Level;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Level, ref _Level, value);
            }
        }
        
        private IdentificationUnitVM (IdentificationUnit model, int level, ReactiveCollection<IdentificationUnit> unitPool)
            : base(model)
	    {
            Level = level;
            UnitPool = unitPool;
            if (UnitPool != null && Level <= MAX_DISPLAY_DEPTH)
            {
                SubUnits = UnitPool
                    .CreateDerivedCollection(iu => new IdentificationUnitVM(iu,this), iu => iu.RelatedUnitID == Model.UnitID, (a,b) => a.Model.UnitID.CompareTo(b.Model.UnitID));
            }
            else
            {
                SubUnits = new ReactiveCollection<IdentificationUnitVM>();
            }
	    }
                  

        public IdentificationUnitVM(IdentificationUnit model, ReactiveCollection<IdentificationUnit> unitPool = null)
            : this(model, 0, unitPool)
        {
            
        }

        public IdentificationUnitVM(IdentificationUnit model, IdentificationUnitVM parent)
            : this(model,parent.Level + 1, parent.UnitPool)
        {
            SelectSubject.Subscribe(parent.SelectSubject);
        }      
        
    }

}
