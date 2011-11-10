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
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;



namespace DiversityPhone.ViewModels
{
    public class EditAnalysisVM:ReactiveObject
    {
       private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion


        public bool _editable;
        public bool Editable { get { return _editable; } set { this.RaiseAndSetIfChanged(x => x.Editable,ref _editable, value); } }

        #region Properties
        private IdentificationUnitAnalysis _Model;
        public IdentificationUnitAnalysis Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value);
            this._Parent = _storage.getIUbyID(_Model.IdentificationUnitID);
            }
        }

        private IdentificationUnit _Parent;

        private IList<Analysis> _Analyses = null;
        public IList<Analysis> Analyses
        {
            get
            {
                if (_Parent == null)
                    throw new NullReferenceException();
                return _Analyses?? (_Analyses = _storage.getPossibleAnalyses(_Parent.TaxonomicGroup));
            }
        }

        private Analysis _SelectedAnalysis;
        public Analysis SelectedAnalysis
        {
            get { return _SelectedAnalysis; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedAnalysis, ref _SelectedAnalysis, value); }
        }

        private IList<AnalysisResult> _AnalysisResults = null;
        public IList<AnalysisResult> AnalysisResults
        {
            get
            {
                if (Model == null)
                    throw new NullReferenceException();
                else return _AnalysisResults ?? (_AnalysisResults = _storage.getPossibleAnalysisResults(Model.AnalysisID));
            }
        }

        public string _SelectedAnalysisResult;
        public string SelectedAnalysisResult
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
        {

            _messenger = messenger;
            _storage = storage;
            this._editable = false;

            var canSave = this.ObservableForProperty(x => x.SelectedAnalysisResult)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => setEdit()),

                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),

                _messenger.Listen<IdentificationUnitAnalysis>(MessageContracts.EDIT)
                    .Subscribe(iua => updateView(iua))
            };
        }



        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<IdentificationUnitAnalysis>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }


        private void setEdit()
        {
            if (Editable == false)
                Editable = true;
            else
                Editable = false;
        }

        private void delete()
        {
            _messenger.SendMessage<IdentificationUnitAnalysis>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void updateModel()
        {
            Model.AnalysisID = this.SelectedAnalysis.AnalysisID;
            Model.AnalysisResult = this.SelectedAnalysisResult;
            Model.AnalysisDate = this.AnalysisDate;
        }

        private void updateView(IdentificationUnitAnalysis iua)
        {
            this.Model = iua;
            this.SelectedAnalysis = _storage.getAnalysis(Model.AnalysisID);
            this.SelectedAnalysisResult = Model.AnalysisResult;
            this.AnalysisDate = Model.AnalysisDate;           
        }
    }
}
