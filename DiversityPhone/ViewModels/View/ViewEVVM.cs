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

        public ReactiveCollection<ImageVM> ImageList { get; private set; }

        public ReactiveCollection<MultimediaObjectVM> AudioList { get; private set; }

        public ReactiveCollection<MultimediaObjectVM> VideoList { get; private set; }

        #endregion

        private ReactiveAsyncCommand getSpecimen = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getProperties = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getImages = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getAudioFiles = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getVideos = new ReactiveAsyncCommand();

        public ViewEVVM()            
        {
            SpecList = getSpecimen.RegisterAsyncFunction(ev => Storage.getSpecimenForEvent(ev as Event).Select(spec => new SpecimenVM(spec)))
                .Do(_ => SpecList.Clear())
                .SelectMany(specs => specs)
                .Do( vm => vm.SelectObservable.Select(v => v.Model.SpecimenID.ToString()).ToNavigation(Page.ViewCS))
                .CreateCollection();
            ValidModel
                .Subscribe(getSpecimen.Execute);

            Properties = getProperties.RegisterAsyncFunction(ev => Storage.getPropertiesForEvent((ev as Event).EventID).Select(prop => new PropertyVM(prop)))
                .Do(_ => Properties.Clear())
                .SelectMany(props => props)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.PropertyID.ToString()).ToNavigation(Page.EditEventProperty,ReferrerType.Event, Current.Model.EventID.ToString()))
                .CreateCollection();
            ValidModel
                .Subscribe(getProperties.Execute);

            ImageList = getImages.RegisterAsyncFunction(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, (ev as Event).EventID, MediaType.Image).Select(im => new ImageVM(im)))
                .Do(_ => ImageList.Clear())
                .SelectMany(images => images)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewImage, ReferrerType.Event,Current.Model.EventID.ToString()))
                .CreateCollection();
            ValidModel.Subscribe(getImages.Execute);

            AudioList = getAudioFiles.RegisterAsyncFunction(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, (ev as Event).EventID, MediaType.Audio).Select(aud=> new MultimediaObjectVM(aud)))
                .Do(_ => AudioList.Clear())
                .SelectMany(audioFiles => audioFiles)
                .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewAudio, ReferrerType.Event, Current.Model.EventID.ToString()))
                .CreateCollection();
            ValidModel.Subscribe(getAudioFiles.Execute);

            VideoList = getVideos.RegisterAsyncFunction(ev => Storage.getMultimediaForObjectAndType(ReferrerType.Event, (ev as Event).EventID, MediaType.Video).Select(vid => new MultimediaObjectVM(vid)))
               .Do(_ => VideoList.Clear())
               .SelectMany(videoFiles => videoFiles)
               .Do(vm => vm.SelectObservable.Select(v => v.Model.Uri.ToString()).ToNavigation(Page.ViewVideo, ReferrerType.Event, Current.Model.EventID.ToString()))
               .CreateCollection();
            ValidModel.Subscribe(getVideos.Execute);

            Add = new ReactiveCommand();
            var addMessageSource =
                Add
                .Select(_ =>
                    {
                        switch (SelectedPivot)
                        {
                            case Pivots.Multimedia:
                                return Page.SelectNewMMO;
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
