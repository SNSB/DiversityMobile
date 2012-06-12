
using ReactiveUI;
using DiversityPhone.Model;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class IdentificationUnitAnalysisVM : ElementVMBase<IdentificationUnitAnalysis>
    {        
        public IdentificationUnitAnalysisVM(IdentificationUnitAnalysis model)
            : base(model)
        {
            
        }
        
        public override string Description
        {
            get { return Model.DisplayText; } 
        }

        public override Icon Icon
        {
            get { return Icon.Analysis; }
        }        
    }
}
