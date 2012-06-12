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

        public IEnumerable<ImageVM> ImageList { get { return _ImageList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<ImageVM>> _ImageList;

        public IEnumerable<MultimediaObjectVM> AudioList { get { return _AudioList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _AudioList;

        public IEnumerable<MultimediaObjectVM> VideoList { get { return _VideoList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _VideoList;
       
    

        #endregion



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
                

            _ImageList = this.ObservableToProperty(
                ValidModel
               .Select(spec => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, spec.CollectionSpecimenID, MediaType.Image))
               .Select(mmos => mmos.Select(mmo => new ImageVM( mmo)))
               .Do(mmos =>
               {
                   foreach (var mmo in mmos)
                   {
                       mmo.SelectObservable
                           .Select(m => m.Model.Uri)
                           .ToNavigation(Page.ViewImage);
                   }
               }), 
               x => x.ImageList);


            _AudioList = this.ObservableToProperty( 
                ValidModel
                .Select(spec => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, spec.CollectionSpecimenID, MediaType.Audio))
                .Select(mmos => mmos.Select(mmo => new MultimediaObjectVM( mmo)))
                .Do(mmos =>
                {
                    foreach (var mmo in mmos)
                    {
                        mmo.SelectObservable
                            .Select(m => m.Model.Uri)
                            .ToNavigation(Page.ViewAudio);
                    }
                }),
                x => x.AudioList);

            _VideoList = this.ObservableToProperty(
                ValidModel
                .Select(spec => Storage.getMultimediaForObjectAndType(ReferrerType.Specimen, spec.CollectionSpecimenID, MediaType.Video))
                .Select(mmos => mmos.Select(mmo => new MultimediaObjectVM( mmo)))
                .Do(mmos =>
                {
                    foreach (var mmo in mmos)
                    {
                        mmo.SelectObservable
                            .Select(m => m.Model.Uri)
                            .ToNavigation(Page.ViewVideo);
                    }
                }),
                x => x.VideoList);
                    

            Messenger.RegisterMessageSource(
                Add
                .Select(_ =>
                {
                    switch (SelectedPivot)
                    {
                        case Pivots.Multimedia:
                            return Page.EditMMO;
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
