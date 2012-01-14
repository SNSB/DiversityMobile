using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reactive.Linq;
using ReactiveUI;
using System.Linq;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;



namespace DiversityPhone.ViewModels
{
    public class EditAnalysisVM : ElementPageViewModel<IdentificationUnitAnalysis>
    {
       private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion


        

        #region Properties
        private ObservableAsPropertyHelper<bool> _IsEditable;
        public bool IsEditable { get { return _IsEditable.Value; } }

        private ObservableAsPropertyHelper<IdentificationUnitAnalysis> _Model;
        public IdentificationUnitAnalysis Model { get { return _Model.Value; } }

        private ObservableAsPropertyHelper<IdentificationUnit> _Parent;
        public IdentificationUnit Parent { get { return _Parent.Value; } }

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

        public AnalysisResult _SelectedAnalysisResult;
        public AnalysisResult SelectedAnalysisResult
        {
            get { return _SelectedAnalysisResult; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedAnalysisResult, ref _SelectedAnalysisResult, value); }
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


        public EditAnalysisVM(IMessageBus messenger, IOfflineStorage storage)
            : base(messenger, false)
        {
            _IsEditable = StateObservable
                .Select(s => s.Context == null) //New IUANs are editable
                .Merge(
                    ToggleEditable
                    .Select(_ => !IsEditable)
                )
                .ToProperty(this, vm => vm.IsEditable);

            ValidModel
                .ToProperty(this, vm => vm.Model);

            var viewUpdate = ValidModel
                .Select(iuan => 
                    {
                        var parent = _storage.getIdentificationUnitByID(iuan.IdentificationUnitID);
                        var analyses = _storage.getPossibleAnalyses(parent.TaxonomicGroup);
                        var selectedAN = (from an in analyses
                                          where an.AnalysisID == iuan.AnalysisID
                                          select an).FirstOrDefault() ?? analyses.First();
                        var results = _storage.getPossibleAnalysisResults(selectedAN.AnalysisID);
                        var selectedResult = (from res in results
                                              where res.Result == iuan.AnalysisResult
                                              select res).FirstOrDefault() ?? results.First();

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
                .Select(u => u.Parent)
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
            viewUpdate
                .Select(u => u.SelectedResult)
                .BindTo(this, x => x.SelectedAnalysisResult);


            
                                     

            var canSave = this.ObservableForProperty(x => x.SelectedAnalysisResult)
                .Select(desc => desc.Value != null)
                .StartWith(false);



            Messenger.RegisterMessageSource(
                Save
                .Do(_ => updateModel())
                .Select(_ => Model),
                MessageContracts.SAVE);

            Messenger.RegisterMessageSource(
                Delete
                .Select(_ => Model),
                MessageContracts.DELETE);

            Messenger.RegisterMessageSource(
                Delete
                .Merge(Save)
                .Select(_ => Message.NavigateBack)
                );
        }       

        private void updateModel()
        {
            Model.AnalysisID = this.SelectedAnalysis.AnalysisID;
            Model.AnalysisResult = this.SelectedAnalysisResult.Result;
            Model.AnalysisDate = this.AnalysisDate;
        }       

        protected override IdentificationUnitAnalysis ModelFromState(PageState s)
        {
            //Existing IUAN
            if (s.Context != null)
            {
                int anID;
                if (int.TryParse(s.Context, out anID))
                {
                    return _storage.getIUANByID(anID);
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
    }
}
