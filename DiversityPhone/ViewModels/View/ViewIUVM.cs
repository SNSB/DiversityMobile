using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;
using System.Linq;
using System.Collections.ObjectModel;
using Funq;
using System.Reactive.Disposables;
using Microsoft.Phone.Reactive;

namespace DiversityPhone.ViewModels
{
    public class ViewIUVM : ViewPageVMBase<IdentificationUnit>
    {
        private ReactiveAsyncCommand getAnalyses = new ReactiveAsyncCommand();

        public enum Pivots
        {
            Subunits,
            Descriptions,            
            Multimedia
        }

        private Container IOC;
        private IVocabularyService Vocabulary;
        private IFieldDataService Storage;

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand Back { get; private set; }

        public ReactiveCommand<IElementVM<IdentificationUnit>> EditCurrent { get; private set; }
        public ReactiveCommand<IElementVM<IdentificationUnit>> SelectUnit { get; private set; }
        #endregion

        #region Properties

        Stack<IElementVM<IdentificationUnit>> unitBackStack = new Stack<IElementVM<IdentificationUnit>>();

        private Pivots _SelectedPivot;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        private SerialDisposable _SubunitListener = new SerialDisposable();
        private ObservableAsPropertyHelper<ReactiveCollection<IdentificationUnitVM>> _Subunits;
        public ReactiveCollection<IdentificationUnitVM> Subunits { get { return _Subunits.Value; } }

        public ReactiveCollection<IdentificationUnitAnalysisVM> Analyses { get; private set; }

        #endregion

        public ViewIUVM(Container ioc)
        {
            IOC = ioc;
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Storage = ioc.Resolve<IFieldDataService>();

            EditCurrent = new ReactiveCommand<IElementVM<IdentificationUnit>>();
            EditCurrent
                .ToMessage(MessageContracts.EDIT);

            SelectUnit = new ReactiveCommand<IElementVM<IdentificationUnit>>();
            SelectUnit
                .Do(vm => unitBackStack.Push(vm))
                .ToMessage(MessageContracts.VIEW);            
                

            _Subunits = this.ObservableToProperty(
                CurrentObservable
                .Select(vm => (vm as IdentificationUnitVM).SubUnits)
                .Do(units => _SubunitListener.Disposable = units.ListenToChanges<IdentificationUnit,IdentificationUnitVM>(iu => iu.RelatedUnitID == Current.Model.UnitID )),
                x => x.Subunits); 

            Analyses = getAnalyses.RegisterAsyncFunction(iu => Storage.getIUANForIU(iu as IdentificationUnit).Select(iuan => new IdentificationUnitAnalysisVM(iuan)))
                .SelectMany(vms => vms)
                .CreateCollection();

            CurrentModelObservable
                .Do(_ => Analyses.Clear())
                .Subscribe(getAnalyses.Execute);
          

            Add = new ReactiveCommand();
            Add.Where(_ => SelectedPivot == Pivots.Subunits)
                .Select(_ => new IdentificationUnitVM( new IdentificationUnit(){ RelatedUnitID = Current.Model.UnitID, SpecimenID = Current.Model.SpecimenID}) as IElementVM<IdentificationUnit>)
                .ToMessage(MessageContracts.EDIT);

            Maps = new ReactiveCommand();
            Maps.Select(_ => Current)
                .ToMessage(MessageContracts.MAPS);
                            

            Back = new ReactiveCommand();
            Back
                .Subscribe(_ => goBack());

        }

        private void goBack()
        {
            if (unitBackStack.Any())
                Messenger.SendMessage(unitBackStack.Pop(), MessageContracts.VIEW);
            else
                Messenger.SendMessage(Page.Previous);
        }
    }
}
