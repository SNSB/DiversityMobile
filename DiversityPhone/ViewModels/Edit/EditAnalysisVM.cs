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

        private ObservableAsPropertyHelper<IdentificationUnitAnalysis> _Model;
        public IdentificationUnitAnalysis Model { get { return _Model.Value; } }

        public ListSelectionHelper<Analysis> Analyses { get; private set; }

        private ListSelectionHelper<AnalysisResult> _Results = new ListSelectionHelper<AnalysisResult>();
        public ListSelectionHelper<AnalysisResult> Results { get { return _Results; } }

        private ObservableAsPropertyHelper<bool> _IsCustomResult;
        public bool IsCustomResult { get { return _IsCustomResult.Value; } }        

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

            _Model = ValidModel.ToProperty(this, x => x.Model);

            Analyses = new ListSelectionHelper<Analysis>();
            _Parent
                .Select(parent => Storage.getPossibleAnalyses(parent.Model.TaxonomicGroup))
                .Subscribe(Analyses.ItemsSubject);
                        
            Analyses.SelectedItemObservable
                .Select(selectedAN => (selectedAN != null) ? Storage.getPossibleAnalysisResults(selectedAN.AnalysisID) : null)
                .Subscribe(Results.ItemsSubject);
            _IsCustomResult = Results.ItemsSubject
                .Where(res => res != null)
                .Select(results => results.Count == 0)
                .ToProperty(this, vm => vm.IsCustomResult);          
            _IsCustomResult
                .Where(custom => custom)
                .Select(_ => String.Empty)
                .BindTo(this, x => x.CustomResult);
        }

        protected override IObservable<bool> CanSave()
        {
            var vocabularyResultValid = Results.SelectedItemObservable
                .Select(result => result != null);

            var customResultValid = this.ObservableForProperty(x => x.CustomResult)
                .Select(change => !string.IsNullOrWhiteSpace(change.Value));

            var resultValid = this.ObservableForProperty(x => x.IsCustomResult)
                .Select(change => change.Value)
                .SelectMany(isCustomResult => (isCustomResult) ? customResultValid : vocabularyResultValid);

            return resultValid;
        }       

        protected override void UpdateModel()
        {
            Current.Model.AnalysisID = Analyses.SelectedItem.AnalysisID;
            Current.Model.AnalysisResult = (IsCustomResult) ? CustomResult : Results.SelectedItem.Result;
            //Current.Model.AnalysisDate = this.AnalysisDate;
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
