namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using DiversityPhone.Services;
    using ReactiveUI.Xaml;
    using System.Linq;

using System.Reactive.Disposables;

    public class ViewEVVM : ElementPageViewModel<Event>
    {
        public enum Pivots
        {
            Specimen,
            Descriptions,
            Multimedia
        }        

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        #endregion

        #region Properties
        private Pivots _SelectedPivot = Pivots.Specimen;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        public ReactiveCollection<SpecimenVM> SpecList { get; private set; }


        public ReactiveCollection<PropertyVM> Properties { get; private set; }        

        public IEnumerable<ImageVM> ImageList { get { return _ImageList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<ImageVM>> _ImageList;

        public IEnumerable<MultimediaObjectVM> AudioList { get { return _AudioList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _AudioList;

        public IEnumerable<MultimediaObjectVM> VideoList { get { return _VideoList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _VideoList;

        #endregion

        private ReactiveAsyncCommand getSpecimen = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getProperties = new ReactiveAsyncCommand();
        public ViewEVVM()            
        {
            SpecList = getSpecimen.RegisterAsyncFunction(ev => Storage.getSpecimenForEvent(ev as Event).Select(spec => new SpecimenVM(spec)))
                .Do(_ => SpecList.Clear())
                .SelectMany(specs => specs)
                .Do( vm => vm.SelectObservable.Select(v => v.Model.CollectionSpecimenID.ToString()).ToNavigation(Page.ViewCS))
                .CreateCollection();

            Properties = getProperties.RegisterAsyncFunction(ev => Storage.getPropertiesForEvent((ev as Event).EventID).Select(prop => new PropertyVM(prop)))
                .Do(_ => Properties.Clear())
                .SelectMany(props => props)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.PropertyID.ToString()).ToNavigation(Page.ViewCS,ReferrerType.Event, Current.Model.EventID.ToString()))
                .CreateCollection();

            _ImageList = this.ObservableToProperty(
                ValidModel
                .Select(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, ev.EventID, MediaType.Image))
                .Select(mmos => mmos.Select(mmo => new ImageVM(mmo)))
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
                .Select(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, ev.EventID, MediaType.Audio))
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
                .Select(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, ev.EventID, MediaType.Video))
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

            Add = new ReactiveCommand();
            var addMessageSource =
                Add
                .Select(_ =>
                    {
                        switch (SelectedPivot)
                        {
                            case Pivots.Multimedia:
                                return Page.EditMMO;
                            case Pivots.Descriptions:
                                return Page.EditEventProperty;
                            case Pivots.Specimen:
                                return Page.EditCS;
                            default:
                                return Page.EditCS;
                        }
                    })
                .Select(p => new NavigationMessage(p, null, ReferrerType.Event, Current.Model.EventID.ToString()));
            Messenger.RegisterMessageSource(addMessageSource);
            Maps = new ReactiveCommand();
            var mapMessageSource =
                Maps
                .Select(_ => new NavigationMessage(Page.LoadedMaps, null, ReferrerType.Event, Current.Model.EventID.ToString()));
            Messenger.RegisterMessageSource(mapMessageSource);
        }       

        //private IList<MultimediaObjectVM> getMMOList(Event ev,MediaType type)
        //{
        //    return new VirtualizingReadonlyViewModelList<MultimediaObject, MultimediaObjectVM>(
        //        Storage.getMultimediaForObjectAndType(ReferrerType.Event,ev.EventID, type),
        //        (model) => new MultimediaObjectVM(Messenger, model, Page.ViewMMO)
        //        );
        //}

        //private IList<ImageVM> getImageList(Event ev)
        //{
        //    return new VirtualizingReadonlyViewModelList<MultimediaObject, ImageVM>(
        //        Storage.getMultimediaForObjectAndType(ReferrerType.Event, ev.EventID, MediaType.Image),
        //        (model) => new ImageVM(Messenger, model, Page.ViewMMO)
        //        );
        //}

        protected override Event ModelFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return Storage.getEventByID(id);
                }
            }
            return null;
        }

        private SerialDisposable model_select = new SerialDisposable();

        protected override ElementVMBase<Event> ViewModelFromModel(Event model)
        {
            var res = new EventVM(model);
            model_select.Disposable = res.SelectObservable
                .Select(vm => vm.Model.EventID.ToString())
                .ToNavigation(Page.EditEV);
            return res;
        }
    }
}
