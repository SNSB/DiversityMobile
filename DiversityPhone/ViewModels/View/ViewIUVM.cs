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

namespace DiversityPhone.ViewModels
{
    public class ViewIUVM : PageViewModel
    {
        public enum Pivots
        {
            Subunits,
            Descriptions,            
            Multimedia
        }

        private Container IOC;
        IVocabularyService Vocabulary;
        IFieldDataService Storage;

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand Back { get; private set; }
        public ReactiveCommand EditCurrent { get; private set; }

        private ReactiveAsyncCommand getAnalyses = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand fetchSubunits = new ReactiveAsyncCommand(null, 2);
        #endregion

        #region Properties

        Stack<IdentificationUnitVM> unitBackStack = new Stack<IdentificationUnitVM>();

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


        private IdentificationUnitVM _Current;

        public IdentificationUnitVM Current
        {
            get
            {
                return _Current;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Current, ref _Current, value);
            }
        }

        CompositeDisposable select_subscription = new CompositeDisposable();

        private ObservableAsPropertyHelper<ReactiveCollection<IdentificationUnitVM>> _Subunits;
        public ReactiveCollection<IdentificationUnitVM> Subunits { get { return _Subunits.Value; } }


        public ReactiveCollection<IdentificationUnitAnalysisVM> Analyses { get { return _Analyses.Value; } }
        private ObservableAsPropertyHelper<ReactiveCollection<IdentificationUnitAnalysisVM>> _Analyses;

        public ReactiveCollection<ImageVM> ImageList { get; private set; }

        public ReactiveCollection<MultimediaObjectVM> AudioList { get; private set; }

        public ReactiveCollection<MultimediaObjectVM> VideoList { get; private set; }
        

        #endregion

        private ReactiveAsyncCommand getImages = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getAudioFiles = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getVideos = new ReactiveAsyncCommand();
        

        public ViewIUVM(Container ioc)
        {
            IOC = ioc;
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Storage = ioc.Resolve<IFieldDataService>();

            DistinctStateObservable
                .Select(s => s.VMContext as IdentificationUnitVM)
                .Where(ctx => ctx != null)
                .BindTo(this, x => x.Current);


            var current = this.ObservableForProperty(x => x.Current).Value().Publish();
            current.Connect();

            current
                .Subscribe((vm) => 
                {
                    select_subscription.Clear();
                    select_subscription.Add(vm.SelectObservable.Subscribe(selectSubUnit));
                    select_subscription.Add(vm.SubUnits.ListenToChanges<IdentificationUnit, IdentificationUnitVM>());
                });

            _Subunits = this.ObservableToProperty(
                current
                .Select(vm => vm.SubUnits),
                x => x.Subunits
                );            

            var valid_model = current.Select(vm => vm.Model).Where(m => m!= null);

            _Analyses = this.ObservableToProperty(
                this.ObservableForProperty(x => x.SelectedPivot)
                .Value()
                .Merge(
                    current 
                    .Select(_ => SelectedPivot)) //If the page is refreshed on the descriptions pivot
                .Where(x => x == Pivots.Descriptions)
                .Select(_ => Current)
                .DistinctUntilChanged()
                .Select(_ => new Subject<IdentificationUnitAnalysisVM>())
                .Select(subject =>
                    {
                        var coll = subject
                            .Do(vm => vm.SelectObservable
                                .Select(v => v.Model.IdentificationUnitAnalysisID.ToString())
                                .ToNavigation(Page.EditIUAN))
                            .CreateCollection();
                        getAnalyses.Execute(subject);
                        return coll;
                    }), x => x.Analyses);

            getAnalyses
                .RegisterAsyncAction(collectionSubject => getAnalysesImpl(Current, collectionSubject as ISubject<IdentificationUnitAnalysisVM>));


          

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

        private void getAnalysesImpl(IElementVM<IdentificationUnit> iuvm, ISubject<IdentificationUnitAnalysisVM> collectionSubject)
        {
            foreach (var iuanVM in Storage
                                    .getIUANForIU(iuvm.Model)                                    
                                    .Select(iuan => new IdentificationUnitAnalysisVM(iuan)))
                collectionSubject.OnNext(iuanVM);
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
