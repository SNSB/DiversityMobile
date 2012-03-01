
using ReactiveUI;
using DiversityPhone.Model;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class IdentificationUnitAnalysisVM : ElementVMBase<IdentificationUnitAnalysis>
    {
        public IdentificationUnitAnalysisVM(IMessageBus msngr, IdentificationUnitAnalysis model)
            : base(msngr, model, Page.EditIUAN)
        {

        }

        public override string Description
        {
            get { return string.Format("{0} : {1}", Model.AnalysisID, Model.AnalysisResult); } //TODO
        }

        public override Icon Icon
        {
            get { return Icon.Analysis; }
        }

        protected override Messages.NavigationMessage NavigationMessage
        {
            get { return new Messages.NavigationMessage(TargetPage, Model.AnalysisID.ToString()); }
        }
    }
}
