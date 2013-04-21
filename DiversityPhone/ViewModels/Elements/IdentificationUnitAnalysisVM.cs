using System;
using ReactiveUI;
using DiversityPhone.Model;
using DiversityPhone.Services;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class IdentificationUnitAnalysisVM : ElementVMBase<IdentificationUnitAnalysis>
    {
        public IdentificationUnitAnalysisVM(IdentificationUnitAnalysis model, IVocabularyService Vocabulary)
            : base(model)
        {
            model.WhenAny(x => x.AnalysisResult, x => x.Value)
                .StartWith(model.AnalysisResult)
                .Subscribe(result =>
                {
                    var an = Vocabulary.getAnalysisByID(Model.AnalysisID);
                    if (an != null)
                    {
                        _Description = string.Format("{0}: {1}{2}", an.DisplayText, result, an.MeasurementUnit);
                        this.RaisePropertyChanged(x => x.Description);
                    }
                });
        }


        private string _Description;

        public override string Description
        {
            get
            {
                return _Description;
            }
        }


        public override Icon Icon
        {
            get { return Icon.Analysis; }
        }
    }
}
