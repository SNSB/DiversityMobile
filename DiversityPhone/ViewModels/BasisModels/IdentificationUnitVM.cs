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
        
        //public bool HasSubUnits { get; private set; }   
        private ReactiveAsyncCommand getSubUnits;

        public IdentificationUnitVM(IdentificationUnit model, int sublevels = 0)
            : base(model)
        {
            if (sublevels > 0)
            {
                getSubUnits = new ReactiveAsyncCommand();
                SubUnits =
                getSubUnits.RegisterAsyncFunction(_ =>
                    {
                        using (var ctx = new DiversityDataContext())
                        {
                            return (from iu in ctx.IdentificationUnits
                                    where iu.RelatedUnitID == model.UnitID
                                    select new IdentificationUnitVM(SelectSubject, iu, sublevels - 1)).ToList();
                        }
                    })
                    .SelectMany(vms => vms)
                    .CreateCollection();
                getSubUnits.Execute(null);
            }
        }

        public IdentificationUnitVM(IObserver<ElementVMBase<IdentificationUnit>> selectObserver, IdentificationUnit model, int sublevels = 0)
            : this(model, sublevels)
        {
            SelectSubject.Subscribe(selectObserver.OnNext);
        }      
        
    }

}
