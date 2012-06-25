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
                .RegisterAsyncAction(subject => fetchSubunitsImpl(subject as ISubject<IdentificationUnitVM>));

            ImageList = getImages.RegisterAsyncFunction(cs => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, (cs as Specimen).CollectionSpecimenID, MediaType.Image).Select(im => new ImageVM(im)))
             .Do(_ => ImageList.Clear())
             .SelectMany(images => images)
             .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewImage, ReferrerType.Specimen, Current.Model.CollectionSpecimenID.ToString()))
             .CreateCollection();
            ValidModel.Subscribe(getImages.Execute);

            AudioList = getAudioFiles.RegisterAsyncFunction(cs => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, (cs as Specimen).CollectionSpecimenID, MediaType.Audio).Select(aud => new MultimediaObjectVM(aud)))
                .Do(_ => AudioList.Clear())
                .SelectMany(audioFiles => audioFiles)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewAudio, ReferrerType.Specimen, Current.Model.CollectionSpecimenID.ToString()))
                .CreateCollection();
            ValidModel.Subscribe(getAudioFiles.Execute);

            VideoList = getVideos.RegisterAsyncFunction(cs => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, (cs as Specimen).CollectionSpecimenID, MediaType.Video).Select(vid => new MultimediaObjectVM(vid)))
               .Do(_ => VideoList.Clear())
               .SelectMany(videoFiles => videoFiles)
               .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewVideo, ReferrerType.Specimen, Current.Model.CollectionSpecimenID.ToString()))
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
                .Select(p => new NavigationMessage(p, null, ReferrerType.Specimen, Current.Model.CollectionSpecimenID.ToString()))
                );
            Maps = new ReactiveCommand();
            var mapMessageSource =
                Maps
                .Select(_ => new NavigationMessage(Page.LoadedMaps, null, ReferrerType.Specimen, Current.Model.DiversityCollectionSpecimenID.ToString()));
            Messenger.RegisterMessageSource(mapMessageSource);
        }

        private void fetchSubunitsImpl(ISubject<IdentificationUnitVM> subject)
        {
            var toplevel = Storage.getTopLevelIUForSpecimen(Current.Model);
            foreach(var top in  toplevel)
            {
                var unit = new IdentificationUnitVM(top,2);
                unit.SelectObservable
                    .Select(vm => vm.Model.UnitID.ToString())
                    .ToNavigation(Page.ViewIU);
                subject.OnNext(unit);
            }
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
                    .Select(vm => vm.Model.CollectionSpecimenID.ToString())
                    .ToNavigation(Page.EditCS);
            }
            else
                model_select.Disposable = null;

            return res;
        }
    }
}
