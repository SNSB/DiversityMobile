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


namespace DiversityPhone.ViewModels
{
   
    public class IdentificationUnitVM : ElementVMBase<IdentificationUnit>
    {        
        public override string Description { get { return Model.WorkingName; } }
        public override Icon Icon { get { return Icon.IdentificationUnit; } }

        public IList<IdentificationUnitVM> SubUnits { get; private set; }
        public bool HasSubUnits { get; private set; }

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.UnitID.ToString()); }
        }

        public IdentificationUnitVM(Container ioc, IdentificationUnit model, Page targetPage, IList<IdentificationUnitVM> subunits = null)
            : base(ioc.Resolve<IMessageBus>(), model,targetPage)
        {          
            SubUnits = subunits;
            HasSubUnits = (subunits != null) ? subunits.Count > 0 : false;
        }

        public static void FillIUVMObservable(ISubject<IdentificationUnitVM> subject, IList<IdentificationUnit> topLevelIUs, Container ioc, Page targetPage, int subunitLevels)
        {
            if(ioc == null)
                throw new ArgumentNullException("ioc");            

            if (subject == null)
                throw new ArgumentNullException("subject");
             
            if (topLevelIUs == null)
                throw new ArgumentNullException("toplevelIUs");
            if (subunitLevels < 1)
                throw new ArgumentOutOfRangeException("subunitLevels");


            IFieldDataService storage = ioc.Resolve<IFieldDataService>();
            if (storage == null)
                throw new ArgumentException("IOC Container does not contain IFieldDataService instance");

            int currentDepth = 0;

            Stack<Queue<IdentificationUnit>> unitLevels = new Stack<Queue<IdentificationUnit>>();
            Stack<IList<IdentificationUnitVM>> vmLevels = new Stack<IList<IdentificationUnitVM>>();
            unitLevels.Push(new Queue<IdentificationUnit>(topLevelIUs));
            vmLevels.Push(null);

            while (unitLevels.Any()) // still units to be processed
            {
                var currentUnits = unitLevels.Peek();               
                if (currentUnits.Any()) //this level still has units to be processed
                {
                    if (currentDepth < subunitLevels) // not yet at the lowest level -> descend
                    {
                        unitLevels.Push(new Queue<IdentificationUnit>(storage.getSubUnits(currentUnits.Peek())));
                        vmLevels.Push(new List<IdentificationUnitVM>());
                        ++currentDepth;
                    }
                    else //lowest level reached -> process leaf units
                    { 
                        var currentVMs = vmLevels.Peek();
                        IdentificationUnit leafUnit;
                        while (currentUnits.Any())
                        {
                            leafUnit = currentUnits.Dequeue();
                            currentVMs.Add(new IdentificationUnitVM(ioc, leafUnit, targetPage, null));
                        }
                    }
                }
                else //all units on this level processed -> ascend
                {
                    unitLevels.Pop();
                    var subunitVMs = vmLevels.Pop();
                    --currentDepth;
                    if (unitLevels.Any()) //Not completely done                        
                    {
                        var currentVMs = vmLevels.Peek();
                        if (currentVMs == null) //toplevel unit done
                            subject.OnNext(new IdentificationUnitVM(ioc, unitLevels.Peek().Dequeue(), targetPage, subunitVMs));
                        else //regular unit done
                            currentVMs.Add(new IdentificationUnitVM(ioc, unitLevels.Peek().Dequeue(), targetPage, subunitVMs));
                    }
                }
            }
        }
    }

}
