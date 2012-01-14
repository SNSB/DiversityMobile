
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;


namespace DiversityPhone.ViewModels
{
    public class MultimediaObjectVM : ElementVMBase<MultimediaObject>
    {        
        public override string Description { get { return Model.ToString(); } }
        public override Icon Icon { get{return ViewModels.Icon.Multimedia; }}

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.Uri); }
        }

        public MultimediaObjectVM(IMessageBus _messenger, MultimediaObject model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
