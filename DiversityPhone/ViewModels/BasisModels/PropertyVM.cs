
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class PropertyVM : ElementVMBase<CollectionEventProperty>
    {       
        public override string Description { get { return Model.ToString(); } }
        public override Icon Icon { get { return ViewModels.Icon.CollectionEventProperty; }  }

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.PropertyID.ToString(), Services.ReferrerType.Event, Model.EventID.ToString()); }
        }

        public PropertyVM(IMessageBus _messenger, CollectionEventProperty model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
