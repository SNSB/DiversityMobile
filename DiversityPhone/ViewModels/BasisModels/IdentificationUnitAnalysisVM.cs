
using ReactiveUI;
using DiversityPhone.Model;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class IdentificationUnitAnalysisVM : ElementVMBase<IdentificationUnitAnalysis>
    {
        public IdentificationUnitAnalysisVM(IMessageBus msngr, IdentificationUnitAnalysis model, Analysis an = null)
            : this(msngr, model, Page.EditIUAN)
        {
            if (an != null)
                _Description = string.Format("{0} : {1} {2}", an.DisplayText, model.AnalysisResult, an.MeasurementUnit); 
        }
        public IdentificationUnitAnalysisVM(IMessageBus msngr, IdentificationUnitAnalysis model, Page page)
            : base(msngr, model, page)
        {
            
        }

        string _Description;
        public override string Description
        {
            get { return _Description; } 
        }

        public override Icon Icon
        {
            get { return Icon.Analysis; }
        }

        protected override Messages.NavigationMessage NavigationMessage
        {
            get { return new Messages.NavigationMessage(TargetPage, Model.IdentificationUnitAnalysisID.ToString()); }
        }
    }
}
