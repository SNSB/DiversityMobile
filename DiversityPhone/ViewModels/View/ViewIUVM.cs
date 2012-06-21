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
    public class ViewIUVM : ElementPageViewModel<IdentificationUnit>
    {
        public enum Pivots
        {
            Subunits,
            Descriptions,            
            Multimedia
        }

        private Container IOC;
        IVocabularyService Vocabulary;

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }

        private ReactiveAsyncCommand getAnalyses = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand fetchSubunits = new ReactiveAsyncCommand(null, 2);
        #endregion

        #region Properties

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

            _Subunits = this.ObservableToProperty(
                this.CurrentObservable
                .Select(_ => new Subject<IdentificationUnitVM>())
                .Select(subject =>
                {
                    var coll = subject
                        .Do(vm => vm.SelectObservable
                            .Select(v => v.Model.UnitID.ToString())
                            .ToNavigation(Page.ViewIU))
                        .CreateCollection();
                    fetchSubunits.Execute(subject);
                    return coll;
                }),
                x => x.Subunits);
            fetchSubunits
                .RegisterAsyncAction(subject => getSubUnitsImpl(subject as ISubject<IdentificationUnitVM>));

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
            ValidModel.Subscribe(getImages.Execute);

            AudioList = getAudioFiles.RegisterAsyncFunction(iu => Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, (iu as IdentificationUnit).UnitID, MediaType.Audio).Select(aud => new MultimediaObjectVM(aud)))
                .Do(_ => AudioList.Clear())
                .SelectMany(audioFiles => audioFiles)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewAudio, ReferrerType.IdentificationUnit ,Current.Model.UnitID.ToString()))
                .CreateCollection();
            ValidModel.Subscribe(getAudioFiles.Execute);

            VideoList = getVideos.RegisterAsyncFunction(iu => Storage.getMultimediaForObjectAndType(ReferrerType.IdentificationUnit, (iu as IdentificationUnit).UnitID, MediaType.Video).Select(vid => new MultimediaObjectVM(vid)))
               .Do(_ => VideoList.Clear())
               .SelectMany(videoFiles => videoFiles)
               .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewVideo, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()))
               .CreateCollection();
            ValidModel.Subscribe(getVideos.Execute);

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
        }

        private void getAnalysesImpl(ElementVMBase<IdentificationUnit> iuvm, ISubject<IdentificationUnitAnalysisVM> collectionSubject)
        {
            foreach (var iuanVM in Storage
                                    .getIUANForIU(iuvm.Model)                                    
                                    .Select(iuan => new IdentificationUnitAnalysisVM(iuan)))
                collectionSubject.OnNext(iuanVM);
        }

        protected override IdentificationUnit ModelFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return Storage.getIdentificationUnitByID(id);
                }               
            }
            else if (s.Referrer != null)
            {
                int parentID;
                if (int.TryParse(s.Referrer, out parentID))
                {
                    if (s.ReferrerType == ReferrerType.IdentificationUnit)
                    {
                        var parent = Storage.getIdentificationUnitByID(parentID);
                        if (parent != null)
                            return new IdentificationUnit()
                            {                            
                                RelatedUnitID = parentID,
                                SpecimenID = parent.SpecimenID,                            
                            };
                    }
                    else if (s.ReferrerType == ReferrerType.Specimen)
                        return new IdentificationUnit()
                        {                        
                            SpecimenID = parentID
                        };
                    
                }
            }                
            return null;
        } 
      
        private void getSubUnitsImpl(ISubject<IdentificationUnitVM> subject)
        {
            var toplevel = Storage.getSubUnits(Current.Model);
            foreach (var top in toplevel)
            {
                var unit = new IdentificationUnitVM(top, 2);
                unit.SelectObservable
                    .Select(vm => vm.Model.UnitID.ToString())
                    .ToNavigation(Page.ViewIU);
                subject.OnNext(unit);
            }                
        }

        SerialDisposable model_select = new SerialDisposable();

        protected override ElementVMBase<IdentificationUnit> ViewModelFromModel(IdentificationUnit model)
        {
            var res = new IdentificationUnitVM(model);
            model_select.Disposable = res.SelectObservable
                .Select(vm => vm.Model.UnitID.ToString())
                .ToNavigation(Page.EditIU);
            return res;
        }
    }
}
