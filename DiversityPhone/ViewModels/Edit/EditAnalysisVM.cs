using System;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using DiversityPhone.Model;
using System.Collections.Generic;
using DiversityPhone.Services;
using ReactiveUI.Xaml;

using System.Reactive.Concurrency;
using DiversityPhone.Interface;
using System.Diagnostics.Contracts;



namespace DiversityPhone.ViewModels
{
    public class EditAnalysisVM : EditPageVMBase<IdentificationUnitAnalysis>
    {           
        readonly IVocabularyService Vocabulary;
        readonly IFieldDataService Storage;

        private ObservableAsPropertyHelper<IElementVM<IdentificationUnit>> _Parent;
        public IElementVM<IdentificationUnit> Parent { get { return _Parent.Value; } }

        private readonly Analysis NoAnalysis = new Analysis() { DisplayText = DiversityResources.Analysis_NoAnalysis };
        public ListSelectionHelper<Analysis> Analyses { get; private set; }


        private readonly AnalysisResult NoResult = new AnalysisResult() { DisplayText = DiversityResources.Analysis_Result_NoResult };
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

        ReactiveAsyncCommand getPossibleResults = new ReactiveAsyncCommand();


        public EditAnalysisVM(
            IFieldDataService Storage,
            IVocabularyService Vocabulary,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            Contract.Requires(Storage != null);
            Contract.Requires(Vocabulary != null);
            this.Storage = Storage;
            this.Vocabulary = Vocabulary;

            
            _Parent = this.ObservableToProperty(
                Messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.VIEW),
                vm => vm.Parent);

            

            Analyses = new ListSelectionHelper<Analysis>();
            Messenger.Listen<IList<Analysis>>()
                .Subscribe(Analyses);

            Analyses.ItemsObservable.Where(items => items != null)
                .CombineLatest(ModelByVisitObservable, (analyses, iuan) =>
                            analyses
                            .Where(an => an.AnalysisID == iuan.AnalysisID)
                            .FirstOrDefault())
                .Where(x => x != null)
                .Subscribe(x => Analyses.SelectedItem = x);
                        
            Analyses
                .Where(an => an != null)
                .SelectMany(selectedAN =>
                    {
                        if(selectedAN != NoAnalysis)
                            return Observable.Start(() => Vocabulary.getPossibleAnalysisResults(selectedAN.AnalysisID) as IList<AnalysisResult>, ThreadPoolScheduler.Instance);
                        else
                            return Observable.Return(Enumerable.Empty<AnalysisResult>().ToList() as IList<AnalysisResult>);
                    })
                .Do(list => list.Insert(0, NoResult))
                .ObserveOn(Dispatcher)
                .Subscribe(Results);

            Results.ItemsObservable
                .Where(results => results != null)
                .CombineLatest(ModelByVisitObservable, (results, iuan) =>
                    results
                    .Where(res => res.Result == iuan.AnalysisResult)
                    .FirstOrDefault())
                .Where(x => x != null)
                .Subscribe(x => Results.SelectedItem = x);

            

            _IsCustomResult = this.ObservableToProperty(
                Results.ItemsObservable
                .Where(res => res != null)
                .Select(results => !results.Any(res => res != NoResult))
                //Don't allow Custom Results until we checked the DB
                .Merge(Analyses.Select(_ => false)), 
                vm => vm.IsCustomResult);

            ModelByVisitObservable                
                .Select(iuan => iuan.AnalysisResult)
                .Merge(
                    this.WhenAny(x => x.IsCustomResult, x => x.Value)
                    .Where(custom => !custom)
                    .Select(_ => String.Empty)
                )
                .Subscribe(x => CustomResult = x);

            Messenger.RegisterMessageSource(
              Save
              .Select(_ => Analyses.SelectedItem),
              MessageContracts.USE);

            CanSave().StartWith(false).Subscribe(CanSaveSubject);
        }

        protected IObservable<bool> CanSave()
        {
            var vocabularyResultValid = Results
                .Select(result => result != NoResult);

            var customResultValid = this.WhenAny(x => x.CustomResult, x => x.Value)
                .Select(change => !string.IsNullOrWhiteSpace(change));

            var resultValid = this.WhenAny(x => x.IsCustomResult, x => x.Value)
                .SelectMany(isCustomResult => (isCustomResult) ? customResultValid : vocabularyResultValid);

            return resultValid;
        }       

        protected override void UpdateModel()
        {
            Current.Model.AnalysisID = Analyses.SelectedItem.AnalysisID;
            Current.Model.AnalysisResult = (IsCustomResult) ? CustomResult : Results.SelectedItem.Result;
        }
    }
}
