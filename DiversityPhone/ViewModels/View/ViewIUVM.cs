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

        Stack<ElementVMBase<IdentificationUnit>> unitBackStack = new Stack<ElementVMBase<IdentificationUnit>>();

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


        public ElementVMBase<IdentificationUnit> Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<ElementVMBase<IdentificationUnit>> _Current;
        

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

            _Current = Messenger.Listen<ElementVMBase<IdentificationUnit>>(MessageContracts.VIEW)
                .ToProperty(this, x => x.Current);

            var valid_model = _Current.Select(vm => vm.Model).Where(m => m!= null);

            _Analyses = this.ObservableToProperty(
                this.ObservableForProperty(x => x.SelectedPivot)
                .Value()
                .Merge(
                    this.ObservableForProperty(x => x.Current) 
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


            ImageList = getImages.RegisterAsyncFunction(iu=> Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, (iu as IdentificationUnit).UnitID, MediaType.Image).Select(im => new ImageVM(im)))
                .Do(_ => ImageList.Clear())
                .SelectMany(images => images)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewImage, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()))
                .CreateCollection();
            valid_model.Subscribe(getImages.Execute);

            AudioList = getAudioFiles.RegisterAsyncFunction(iu => Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, (iu as IdentificationUnit).UnitID, MediaType.Audio).Select(aud => new MultimediaObjectVM(aud)))
                .Do(_ => AudioList.Clear())
                .SelectMany(audioFiles => audioFiles)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewAudio, ReferrerType.IdentificationUnit ,Current.Model.UnitID.ToString()))
                .CreateCollection();
            valid_model.Subscribe(getAudioFiles.Execute);

            VideoList = getVideos.RegisterAsyncFunction(iu => Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, (iu as IdentificationUnit).UnitID, MediaType.Video).Select(vid => new MultimediaObjectVM(vid)))
               .Do(_ => VideoList.Clear())
               .SelectMany(videoFiles => videoFiles)
               .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewVideo, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()))
               .CreateCollection();
            valid_model.Subscribe(getVideos.Execute);

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

        private void getAnalysesImpl(ElementVMBase<IdentificationUnit> iuvm, ISubject<IdentificationUnitAnalysisVM> collectionSubject)
        {
            foreach (var iuanVM in Storage
                                    .getIUANForIU(iuvm.Model)                                    
                                    .Select(iuan => new IdentificationUnitAnalysisVM(iuan)))
                collectionSubject.OnNext(iuanVM);
        }
        
        private void selectSubUnit(ElementVMBase<IdentificationUnit> unit)
        {
            unitBackStack.Push(Current);
            Messenger.SendMessage<ElementVMBase<IdentificationUnit>>(unit, MessageContracts.VIEW);
        }

        private void goBack()
        {
            if (unitBackStack.Any())
            {
                var prev = unitBackStack.Pop();
                Messenger.SendMessage<ElementVMBase<IdentificationUnit>>(prev, MessageContracts.VIEW);
            }
            else
                Messenger.SendMessage(Page.Previous);
        }
    }
}
