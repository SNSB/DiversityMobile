using System;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using DiversityPhone.Model;
using System.Collections.Generic;
using DiversityPhone.Services;



namespace DiversityPhone.ViewModels
{
    public class EditAnalysisVM : EditElementPageVMBase<IdentificationUnitAnalysis>
    { 
        #region Properties
        private ObservableAsPropertyHelper<IdentificationUnitVM> _Parent;
        public IdentificationUnitVM Parent { get { return _Parent.Value; } }

        private ObservableAsPropertyHelper<IList<Analysis>> _Analyses;
        public IList<Analysis> Analyses
        {
            get
            {
                return _Analyses.Value;
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


        public EditAnalysisVM()
        {            
            var viewUpdate = ValidModel
                .Select(iuan => 
                    {
                        var parent = Storage.getIdentificationUnitByID(iuan.IdentificationUnitID);
                        var analyses = Storage.getPossibleAnalyses(parent.TaxonomicGroup);
                        var selectedAN = (from an in analyses
                                          where an.AnalysisID == iuan.AnalysisID
                                          select an).FirstOrDefault() ?? analyses.First();
                        var results = Storage.getPossibleAnalysisResults(selectedAN.AnalysisID);                        
                        var selectedResult = (from res in results
                                              where res.Result == iuan.AnalysisResult
                                              select res).FirstOrDefault() ?? results.FirstOrDefault();

                        return new {
                            Parent = parent,
                            Analyses = analyses,
                            SelectedAN = selectedAN,
                            Results = results,
                            SelectedResult = selectedResult
                        };
                    })                
                .Publish();
            viewUpdate.Connect();

            _Parent = viewUpdate
                .Select(u => new IdentificationUnitVM(Messenger,u.Parent, Services.Page.Current))
                .ToProperty(this, vm => vm.Parent);
            _Analyses = viewUpdate
                .Select(u => u.Analyses)
                .ToProperty(this, vm => vm.Analyses);
            viewUpdate
                .Select(u => u.SelectedAN)
                .BindTo(this, x => x.SelectedAnalysis);
            _AnalysisResults = viewUpdate
                .Select(u => u.Results)
                .ToProperty(this, vm => vm.AnalysisResults);
            _IsCustomResult = viewUpdate
                .Select(u => u.Results.Count == 0)
                .ToProperty(this, vm => vm.IsCustomResult);
            viewUpdate
                .Select(u => u.SelectedResult)
                .BindTo(this, x => x.SelectedAnalysisResult);
            viewUpdate
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
