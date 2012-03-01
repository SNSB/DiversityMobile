using System;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using DiversityPhone.Model;
using System.Collections.Generic;
using DiversityPhone.Services;
using ReactiveUI.Xaml;



namespace DiversityPhone.ViewModels
{
    public class EditAnalysisVM : EditElementPageVMBase<IdentificationUnitAnalysis>
    { 
        #region Properties


        private ObservableAsPropertyHelper<IdentificationUnitVM> _Parent;
        public IdentificationUnitVM Parent { get { return _Parent.Value; } }


        public IdentificationUnitAnalysis Model { get { return ValidModel.First(); } }        

        private ObservableAsPropertyHelper<IList<Analysis>> _Analyses;
        public IList<Analysis> Analyses
        {
            get
            {
                return (_Analyses != null) ? _Analyses.Value : null;
            }
        }

        private Analysis _SelectedAnalysis;
        public Analysis SelectedAnalysis
        {
            get { return _SelectedAnalysis; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedAnalysis, ref _SelectedAnalysis, value); }
        }

        private ObservableAsPropertyHelper<IList<AnalysisResult>> _AnalysisResults = null;
        public IList<AnalysisResult> AnalysisResults
        {
            get
            {
                return _AnalysisResults.Value;
            }
        }

        private ObservableAsPropertyHelper<bool> _IsCustomResult;
        public bool IsCustomResult { get { return _IsCustomResult.Value; } }

        public AnalysisResult _SelectedAnalysisResult;
        public AnalysisResult SelectedAnalysisResult
        {
            get { return _SelectedAnalysisResult; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedAnalysisResult, ref _SelectedAnalysisResult, value); }
        }

        private string _CustomResult;
        public string CustomResult
        {
            get
            {
                return _CustomResult;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CustomResult, ref _CustomResult, value);
            }
        }        

        public DateTime _AnalysisDate;
        public DateTime AnalysisDate
        {
            get { return _AnalysisDate; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.AnalysisDate, value);
            }
        }
        #endregion

        ReactiveAsyncCommand getPossibleResults = new ReactiveAsyncCommand();


        public EditAnalysisVM()
            : base(false)
        {
            
                


            _Parent = ValidModel
                .Select(iuan => Storage.getIdentificationUnitByID(iuan.IdentificationUnitID))
                .Select(parent => new IdentificationUnitVM(Messenger,parent, Services.Page.Current))
                .ToProperty(this, vm => vm.Parent);

            _Analyses = _Parent
                .Select(parent => Storage.getPossibleAnalyses(parent.Model.TaxonomicGroup))
                .ToProperty(this, vm => vm.Analyses);

            this.ObservableForProperty(x => x.Analyses)
                .Value()                
                .Where(analyses => analyses != null)                                
                .Select(analyses => (from an in analyses                                          
                                    where an.AnalysisID == Model.AnalysisID
                                    select an).FirstOrDefault() ?? analyses.FirstOrDefault())
                .BindTo(this, x => x.SelectedAnalysis);

            _AnalysisResults = this.ObservableForProperty(x => x.SelectedAnalysis)
                .Select(change => change.Value)
                .Select(selectedAN => (selectedAN != null) ? Storage.getPossibleAnalysisResults(selectedAN.AnalysisID) : null)                
                .ToProperty(this, vm => vm.AnalysisResults);
            _IsCustomResult = _AnalysisResults
                .Where(res => res != null)
                .Select(results => results.Count == 0)
                .ToProperty(this, vm => vm.IsCustomResult);
            this.ObservableForProperty(x => x.AnalysisResults) 
                .Select(change => change.Value)
                .Where(ars => ars != null)
                .Select(results => (from res in results
                                    where res.Result == Model.AnalysisResult
                                    select res).FirstOrDefault() ?? results.FirstOrDefault())
                .BindTo(this, x => x.SelectedAnalysisResult);
            _IsCustomResult
                .Where(custom => custom)
                .Select(_ => String.Empty)
                .BindTo(this, x => x.CustomResult);
        }

        protected override IObservable<bool> CanSave()
        {
            var vocabularyResultValid = this.ObservableForProperty(x => x.SelectedAnalysisResult)
                .Select(change => change.Value != null);

            var customResultValid = this.ObservableForProperty(x => x.CustomResult)
                .Select(change => !string.IsNullOrWhiteSpace(change.Value));

            var resultValid = this.ObservableForProperty(x => x.IsCustomResult)
                .Select(change => change.Value)
                .SelectMany(isCustomResult => (isCustomResult) ? customResultValid : vocabularyResultValid);

            return resultValid;
        }       

        protected override void UpdateModel()
        {
            Current.Model.AnalysisID = this.SelectedAnalysis.AnalysisID;
            Current.Model.AnalysisResult = (IsCustomResult) ? CustomResult : SelectedAnalysisResult.Result;
            Current.Model.AnalysisDate = this.AnalysisDate;
        }       

        protected override IdentificationUnitAnalysis ModelFromState(PageState s)
        {
            //Existing IUAN
            if (s.Context != null)
            {
                int anID;
                if (int.TryParse(s.Context, out anID))
                {
                    return Storage.getIUANByID(anID);
                }                        
            }
            //New IUAN
            if (s.ReferrerType == ReferrerType.IdentificationUnit && s.Referrer != null)
            {
                int unitID;
                if (int.TryParse(s.Referrer, out unitID))
                {
                    return new IdentificationUnitAnalysis()
                    {
                        IdentificationUnitID = unitID,
                    };
                }
            }
            return null;
        }

        protected override ElementVMBase<IdentificationUnitAnalysis> ViewModelFromModel(IdentificationUnitAnalysis model)
        {
            return new IUAnalysisVM(Messenger, model, Page.Current);
        }
    }
}
