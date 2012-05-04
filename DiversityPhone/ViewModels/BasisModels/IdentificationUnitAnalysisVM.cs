
using ReactiveUI;
using DiversityPhone.Model;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class IdentificationUnitAnalysisVM : ElementVMBase<IdentificationUnitAnalysis>
    {
        public IdentificationUnitAnalysisVM(IMessageBus msngr, IdentificationUnitAnalysis model)
            : this(msngr, model, Page.EditIUAN)
        {
           
        }
        public IdentificationUnitAnalysisVM(IMessageBus msngr, IdentificationUnitAnalysis model, Page page)
            : base(msngr, model, page)
        {
            
        }

        string _Description;
        public override string Description
        {
            get { return Model.DisplayText; } 
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
