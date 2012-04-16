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
        
        public IList<SpecimenVM> SpecList { get { return _SpecList.Value; } }
        private ObservableAsPropertyHelper<IList<SpecimenVM>> _SpecList;

        public IList<PropertyVM> Properties { get { return _Properties.Value; } }
        private ObservableAsPropertyHelper<IList<PropertyVM>> _Properties;

        public IEnumerable<ImageVM> ImageList { get { return _ImageList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<ImageVM>> _ImageList;

        public IEnumerable<MultimediaObjectVM> AudioList { get { return _AudioList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _AudioList;

        public IEnumerable<MultimediaObjectVM> VideoList { get { return _VideoList.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<MultimediaObjectVM>> _VideoList;

        #endregion


        public ViewEVVM()            
        {
            _SpecList = ValidModel               
                .Select(ev => getSpecimenList(ev))
                .ToProperty(this, x => x.SpecList);

            _Properties = ValidModel
                .Select(ev => Storage.getPropertiesForEvent(ev).Select(p => new PropertyVM(Messenger, p, Page.EditEventProperty)).ToList() as IList<PropertyVM>)
                .ToProperty(this, x => x.Properties);

            _ImageList = ValidModel
                .Select(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, ev.EventID, MediaType.Image))
                .Select(mmos => mmos.Select(mmo => new ImageVM(Messenger, mmo, Page.ViewImage)))
                .ToProperty(this, x => x.ImageList);

            _AudioList = ValidModel
                .Select(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, ev.EventID, MediaType.Audio))
                .Select(mmos => mmos.Select(mmo => new MultimediaObjectVM(Messenger, mmo, Page.ViewAudio)))
                .ToProperty(this, x => x.AudioList);

            _VideoList = ValidModel
                .Select(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, ev.EventID, MediaType.Video))
                .Select(mmos => mmos.Select(mmo => new MultimediaObjectVM(Messenger, mmo, Page.ViewVideo)))
                .ToProperty(this, x => x.VideoList);

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

        }

        private IList<SpecimenVM> getSpecimenList(Event ev)
        {
            return new VirtualizingReadonlyViewModelList<Specimen, SpecimenVM>(
                Storage.getSpecimenForEvent(ev),
                (model) => new SpecimenVM(Messenger, model, Page.ViewCS)
                );
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

        protected override ElementVMBase<Event> ViewModelFromModel(Event model)
        {
            return new EventVM(Messenger, model, Page.EditEV);
        }
    }
}
