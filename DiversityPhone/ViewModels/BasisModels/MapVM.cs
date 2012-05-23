

using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public class MapVM : ElementVMBase<Map>
    {        
        public override string Description { get { return Model.Description; } }
        public override Icon Icon { get { return Icon.Map; } }

        private ReferrerType _referrerType=ReferrerType.None;
        public ReferrerType ReferrerType { get { return _referrerType; } }
        private string _referrer=null;
        public string Referrer { get { return _referrer; } }

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.ServerKey, ReferrerType, Referrer); }
        }

        public MapVM(IMessageBus _messenger, Map model, Page targetPage)
            : base(_messenger, model, targetPage)
        {
            _referrerType = ReferrerType.None;
            _referrer = null;
            //Select.Subscribe
        }

        public MapVM(IMessageBus _messenger, Map model, Page targetPage, ReferrerType refType, string referrer)
            : base(_messenger, model, targetPage)
        {
            _referrerType = refType;
            _referrer= referrer;
        }

    }
}
