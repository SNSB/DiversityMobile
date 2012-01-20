namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using System.Reactive.Linq;
    using DiversityPhone.Services;
    using ReactiveUI.Xaml;

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

        public IList<MultimediaObjectVM> MMOList { get { return _MMOList.Value; } }
        private ObservableAsPropertyHelper<IList<MultimediaObjectVM>> _MMOList;

        #endregion




        public ViewEVVM()            
        {
            _SpecList = ValidModel               
                .Select(ev => getSpecimenList(ev))
                .ToProperty(this, x => x.SpecList);

            _MMOList = ValidModel
               .Select(ev => getMMOList(ev))
               .ToProperty(this, x => x.MMOList);

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

        private IList<MultimediaObjectVM> getMMOList(Event ev)
        {
            return new VirtualizingReadonlyViewModelList<MultimediaObject, MultimediaObjectVM>(
                Storage.getMultimediaForObject(ReferrerType.Event,ev.EventID),
                (model) => new MultimediaObjectVM(Messenger, model, Page.ViewMMO)
                );
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

        protected override ElementVMBase<Event> ViewModelFromModel(Event model)
        {
            return new EventVM(Messenger, model, Page.EditEV);
        }
    }
}
