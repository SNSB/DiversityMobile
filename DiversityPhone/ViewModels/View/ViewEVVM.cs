namespace DiversityPhone.ViewModels {
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    public class ViewEVVM : ViewPageVMBase<Event> {
        private readonly IFieldDataService Storage;

        public enum Pivots {
            Specimen,
            Descriptions,
            Multimedia
        }

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }

        public ReactiveCommand<IElementVM<Event>> EditEvent { get; private set; }
        public ReactiveCommand<IElementVM<EventProperty>> SelectProperty { get; private set; }
        public ReactiveCommand<IElementVM<Specimen>> SelectSpecimen { get; private set; }
        #endregion

        #region Properties
        private Pivots _SelectedPivot = Pivots.Specimen;
        public Pivots SelectedPivot {
            get {
                return _SelectedPivot;
            }
            set {
                this.RaiseAndSetIfChanged(vm => vm.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        public ReactiveCollection<SpecimenVM> SpecList { get; private set; }

        public ReactiveCollection<PropertyVM> PropertyList { get; private set; }

        public ElementMultimediaVM MultimediaList { get; private set; }

        #endregion

        private ReactiveAsyncCommand getSpecimen = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getProperties = new ReactiveAsyncCommand();

        public ViewEVVM(
            IFieldDataService Storage,
            ElementMultimediaVM MultimediaList
            ) {
            this.Storage = Storage;

            //Current
            EditEvent = new ReactiveCommand<IElementVM<Event>>();
            EditEvent
                .ToMessage(Messenger, MessageContracts.EDIT);

            //Specimen
            SpecList = getSpecimen.RegisterAsyncFunction(ev => Storage.getSpecimenForEvent(ev as Event).Select(spec => new SpecimenVM(spec)))
                .SelectMany(specs => specs)
                .CreateCollection();
            SpecList.ListenToChanges<Specimen, SpecimenVM>(spec => spec.EventID == Current.Model.EventID);

            CurrentModelObservable
                .Do(_ => SpecList.Clear())
                .Subscribe(getSpecimen.Execute);

            SelectSpecimen = new ReactiveCommand<IElementVM<Specimen>>();
            SelectSpecimen
                .ToMessage(Messenger, MessageContracts.VIEW);

            //Properties
            PropertyList = getProperties.RegisterAsyncFunction(ev => Storage.getPropertiesForEvent((ev as Event).EventID).Select(prop => new PropertyVM(prop)))
                .SelectMany(props => props)
                .CreateCollection();
            PropertyList.ListenToChanges<EventProperty, PropertyVM>(p => p.EventID == Current.Model.EventID);

            CurrentModelObservable
                .Do(_ => PropertyList.Clear())
                .Do(_ => Messenger.SendMessage(PropertyList.Select(vm => vm.Model.PropertyID), VMMessages.USED_EVENTPROPERTY_IDS))
                .Subscribe(getProperties.Execute);

            SelectProperty = new ReactiveCommand<IElementVM<EventProperty>>();
            SelectProperty
                .ToMessage(Messenger, MessageContracts.EDIT);

            //Multimedia
            this.MultimediaList = MultimediaList;

            CurrentModelObservable
                .Select(m => m as IMultimediaOwner)
                .Subscribe(MultimediaList);

            //Add New
            Add = new ReactiveCommand();
            Add.Where(_ => SelectedPivot == Pivots.Specimen)
                .Select(_ => new SpecimenVM(new Specimen() { EventID = Current.Model.EventID }) as IElementVM<Specimen>)
                .ToMessage(Messenger, MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Descriptions)
                .Select(_ => new PropertyVM(new EventProperty() { EventID = Current.Model.EventID }) as IElementVM<EventProperty>)
                .ToMessage(Messenger, MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Multimedia)
                .Subscribe(MultimediaList.AddMultimedia.Execute);

            //Maps
            Maps = new ReactiveCommand();
            Maps
                .Select(_ => Current.Model as ILocalizable)
                .ToMessage(Messenger, MessageContracts.VIEW);
        }
    }
}
