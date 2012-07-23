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


        private SerialDisposable select_registration = new SerialDisposable();
        private ISubject<ElementVMBase<IdentificationUnit>> select_subject = new Subject<ElementVMBase<IdentificationUnit>>();
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


        public ReactiveCollection<IdentificationUnitVM> UnitList { get; private set; }
        
        
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

            fetchSubunits
                .RegisterAsyncAction(subject => fetchSubunitsImpl(subject as ISubject<IdentificationUnitVM>));

            var unitList = new Subject<IdentificationUnitVM>();
            UnitList = unitList
                .CreateCollection();
            fetchSubunits.Execute(unitList);
            UnitList.ListenToChanges(vm => vm.Model.RelatedUnitID == null);
                

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

        private void fetchSubunitsImpl(ISubject<IdentificationUnitVM> collection)
        {
            var subunits = new Queue<IdentificationUnit>(Storage.getIUForSpecimen(Current.Model.SpecimenID));
            var collection_index = new Dictionary<int, IdentificationUnitVM>();          


            while(subunits.Any())
            {
                var unit = subunits.Dequeue();
                if (unit.RelatedUnitID == null)
                {
                    var uvm = new IdentificationUnitVM(unit);
                    uvm.SelectObservable
                        .Subscribe(select_subject);

                    collection.OnNext(uvm);
                }
                else if (collection_index.ContainsKey(unit.RelatedUnitID.Value))
                {
                    var parentvm = collection_index[unit.RelatedUnitID.Value];

                    var uvm = new IdentificationUnitVM(unit, parentvm);

                    parentvm.SubUnits.Add(uvm);
                }
                else
                    subunits.Enqueue(unit);
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
                    .Select(vm => vm.Model.SpecimenID.ToString())
                    .ToNavigation(Page.EditCS);
            }
            else
                model_select.Disposable = null;

            return res;
        }

        public override void Activate()
        {
            select_registration.Disposable = select_subject.Take(1)
                .ToNavigation(Page.ViewIU);
        }

        public override void Deactivate()
        {
            select_registration.Disposable = null;
        }
    }
}
