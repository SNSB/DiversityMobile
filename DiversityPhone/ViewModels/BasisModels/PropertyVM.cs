
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class PropertyVM : ElementVMBase<EventProperty>
    {       
        public override string Description { get { return Model.DisplayText; } }
        public override Icon Icon { get { return ViewModels.Icon.CollectionEventProperty; }  }       

        public PropertyVM(EventProperty model)
            : base( model)
        {

        }
    }
}
