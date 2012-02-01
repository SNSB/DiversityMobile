

using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public class MapVM : ElementVMBase<Map>
    {        
        public override string Description { get { return Model.ToString(); } }
        public override Icon Icon { get { return Icon.Map; } }

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.Uri); }
        }

        public MapVM(IMessageBus _messenger, Map model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
