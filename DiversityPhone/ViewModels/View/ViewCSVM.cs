using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Linq;
using System.Reactive.Subjects;
using Funq;
using System.Reactive.Disposables;
namespace DiversityPhone.ViewModels
{
   

    public class ViewCSVM : ElementPageViewModel<Specimen>
    {
        ReactiveCollection<IdentificationUnit> unitPool = new ListeningReactiveCollection<IdentificationUnit>();  

        private Container IOC;
        public enum Pivots
        {
            Units,
            Multimedia
        }
     
        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }

        private ReactiveAsyncCommand fetchSubunits = new ReactiveAsyncCommand(null);
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
        
        public ReactiveCollection<IdentificationUnitVM> UnitList { get { return _UnitList.Value; } }
        private ObservableAsPropertyHelper<ReactiveCollection<IdentificationUnitVM>> _UnitList;
        
        public ReactiveCollection<ImageVM> ImageList { get; private set; }

        public ReactiveCollection<MultimediaObjectVM> AudioList { get; private set; }

        public ReactiveCollection<MultimediaObjectVM> VideoList { get; private set; }

        #endregion

        private ReactiveAsyncCommand getImages = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getAudioFiles = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getVideos = new ReactiveAsyncCommand();

        public ViewCSVM(Container ioc) 
            :base(false)
        {
            IOC = ioc;
            Add = new ReactiveCommand();

            _UnitList = this.ObservableToProperty(
                this.CurrentObservable
                .Select(_ => new Subject<IdentificationUnitVM>())
                .Select(subject =>
                    {
                        var coll = subject.CreateCollection();
                        fetchSubunits.Execute(subject);
                        return coll;
                    }), x => x.UnitList);
                
            fetchSubunits
                .RegisterAsyncFunction(subject => fetchSubunitsImpl(subject as ISubject<IdentificationUnitVM>))
                .SelectMany(ius => ius)
                .Subscribe(unitPool.Add);

            ImageList = getImages.RegisterAsyncFunction(cs => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, (cs as Specimen).SpecimenID, MediaType.Image).Select(im => new ImageVM(im)))
             .Do(_ => {if(ImageList != null) ImageList.Clear();})
             .SelectMany(images => images)
             .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewImage, ReferrerType.Specimen, Current.Model.SpecimenID.ToString()))
             .CreateCollection();
            ValidModel.Subscribe(getImages.Execute);

            AudioList = getAudioFiles.RegisterAsyncFunction(cs => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, (cs as Specimen).SpecimenID, MediaType.Audio).Select(aud => new MultimediaObjectVM(aud)))
                .Do(_ => { if (AudioList != null) AudioList.Clear(); })
                .SelectMany(audioFiles => audioFiles)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewAudio, ReferrerType.Specimen, Current.Model.SpecimenID.ToString()))
                .CreateCollection();
            ValidModel.Subscribe(getAudioFiles.Execute);

            VideoList = getVideos.RegisterAsyncFunction(cs => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, (cs as Specimen).SpecimenID, MediaType.Video).Select(vid => new MultimediaObjectVM(vid)))
               .Do(_ => { if (VideoList != null) VideoList.Clear(); })
               .SelectMany(videoFiles => videoFiles)
               .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewVideo, ReferrerType.Specimen, Current.Model.SpecimenID.ToString()))
               .CreateCollection();
            ValidModel.Subscribe(getVideos.Execute);    

            Messenger.RegisterMessageSource(
                Add
                .Select(_ =>
                {
                    switch (SelectedPivot)
                    {
                        case Pivots.Multimedia:
                            return Page.SelectNewMMO;
                        case Pivots.Units:
                        default:
                            return Page.EditIU;
                    }
                })
                .Select(p => new NavigationMessage(p, null, ReferrerType.Specimen, Current.Model.SpecimenID.ToString()))
                );
            Maps = new ReactiveCommand();
            var mapMessageSource =
                Maps
                .Select(_ => new NavigationMessage(Page.LoadedMaps, null, ReferrerType.Specimen, Current.Model.DiversityCollectionSpecimenID.ToString()));
            Messenger.RegisterMessageSource(mapMessageSource);
        }

        private IEnumerable<IdentificationUnit> fetchSubunitsImpl(ISubject<IdentificationUnitVM> subject)
        {
            var subunits = Storage.getIUForSpecimen(Current.Model.SpecimenID);
            var toplevel = Storage.getTopLevelIUForSpecimen(Current.Model.SpecimenID);
                      
            foreach(var top in toplevel)
            {
                var unit = new IdentificationUnitVM(top,unitPool);
                unit.SelectObservable
                    .Select(vm => vm.Model.UnitID.ToString())
                    .ToNavigation(Page.ViewIU);
                subject.OnNext(unit);
            }
            return subunits;
        }       

        protected override Specimen ModelFromState(PageState s)
        { 

            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return Storage.getSpecimenByID(id);
                }
            }            
            return null;
        }

        SerialDisposable model_select = new SerialDisposable();

        protected override ElementVMBase<Specimen> ViewModelFromModel(Specimen model)
        {
            var res = new SpecimenVM(model);
            if (!model.IsObservation())
            {
                res.SelectObservable
                    .Select(vm => vm.Model.SpecimenID.ToString())
                    .ToNavigation(Page.EditCS);
            }
            else
                model_select.Disposable = null;

            return res;
        }
    }
}
