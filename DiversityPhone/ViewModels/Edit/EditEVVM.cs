using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DiversityPhone.ViewModels
{
    public class EditEVVM : EditPageVMBase<Event>
    {
        private readonly ILocationService Geolocation;

        #region Properties

        private string _LocalityDescription;

        public string LocalityDescription
        {
            get { return _LocalityDescription; }
            set { this.RaiseAndSetIfChanged(x => x.LocalityDescription, ref _LocalityDescription, value); }
        }

        private string _HabitatDescription;

        public string HabitatDescription
        {
            get { return _HabitatDescription; }
            set { this.RaiseAndSetIfChanged(x => x.HabitatDescription, ref _HabitatDescription, value); }
        }

        private DateTime _CollectionDate;

        public DateTime CollectionDate
        {
            get { return _CollectionDate; }
            set { this.RaiseAndSetIfChanged(x => x.CollectionDate, ref _CollectionDate, value); }
        }

        #endregion Properties

        public EditEVVM(
            ILocationService Geolocation
            )
        {
            Contract.Requires(Geolocation != null);
            this.Geolocation = Geolocation;

            ModelByVisitObservable
                .Select(ev => ev.CollectionDate)
                .Subscribe(date => CollectionDate = date);

            ModelByVisitObservable
                .Select(ev => ev.LocalityDescription)
                .Subscribe(x => LocalityDescription = x);

            ModelByVisitObservable
                .Select(ev => ev.HabitatDescription)
                .Subscribe(x => HabitatDescription = x);

            CanSave().Subscribe(CanSaveSubject.OnNext);
        }

        protected override async Task UpdateModel()
        {
            if (!Current.Model.IsLocalized())
            {
                Current.Model.SetCoordinates(Geolocation.Location().FirstOrDefault() ?? Coordinate.Unknown);
            }
            Current.Model.LocalityDescription = LocalityDescription;
            Current.Model.HabitatDescription = HabitatDescription;
            Current.Model.CollectionDate = CollectionDate;
        }

        protected IObservable<bool> CanSave()
        {
            return this.ObservableForProperty(x => x.LocalityDescription)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);
        }
    }
}