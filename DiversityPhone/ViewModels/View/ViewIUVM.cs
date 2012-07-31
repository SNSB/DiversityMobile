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
            var addMessageSource = 
                Add
                .Select(_ =>
                    {
                        switch(SelectedPivot)
                        {
                            case Pivots.Descriptions:
                                return Page.EditIUAN;
                            case Pivots.Multimedia:
                                return Page.SelectNewMMO;                           
                            case Pivots.Subunits:
                                return Page.EditIU;
                            default:
                                return Page.EditIU;
                        }
                    })
                .Select(p => new NavigationMessage(p,null, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()));
            Messenger.RegisterMessageSource(addMessageSource);

            Maps = new ReactiveCommand();
            var mapMessageSource =
                Maps
                .Select(_ => new NavigationMessage(Page.LoadedMaps, null, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()));
            Messenger.RegisterMessageSource(mapMessageSource);

            Back = new ReactiveCommand();
            Back
                .Subscribe(_ => goBack());

        }

        private void selectSubUnit(IElementVM<IdentificationUnit> unit)
        {
            unitBackStack.Push(Current);
            Messenger.SendMessage<IElementVM<IdentificationUnit>>(unit, MessageContracts.VIEW);
        }

        private void goBack()
        {
            if (unitBackStack.Any())
            {
                var prev = unitBackStack.Pop();
                Messenger.SendMessage<IElementVM<IdentificationUnit>>(prev, MessageContracts.VIEW);
            }
            else
                Messenger.SendMessage(Page.Previous);
        }
    }
}
