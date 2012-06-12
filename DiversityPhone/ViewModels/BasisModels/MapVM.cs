

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

        public MapVM(Map model)
            : base(model)
        {
            _referrerType = ReferrerType.None;
            _referrer = null;
            //Select.Subscribe
        }

        public MapVM(Map model, ReferrerType refType, string referrer)
            : base(model)
        {
            _referrerType = refType;
            _referrer= referrer;
        }

    }
}
