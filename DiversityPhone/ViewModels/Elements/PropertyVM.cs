
using ReactiveUI;
using DiversityPhone.Model;
using System;

namespace DiversityPhone.ViewModels
{
    public class PropertyVM : ElementVMBase<EventProperty>
    {       
        public override string Description { get { return Model.DisplayText; } }
        public override Icon Icon { get { return ViewModels.Icon.CollectionEventProperty; }  }       

        public PropertyVM(EventProperty model)
            : base( model)
        {
            model.ObservableForProperty(x => x.DisplayText).Subscribe(_ => this.RaisePropertyChanged(x => x.Description));
        }
    }
}
