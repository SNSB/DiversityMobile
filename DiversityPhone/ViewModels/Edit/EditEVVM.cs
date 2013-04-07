using System;
using ReactiveUI;
using DiversityPhone.Model;
using System.Reactive.Linq;
using DiversityPhone.Services;

using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Diagnostics.Contracts;

namespace DiversityPhone.ViewModels
{
    public class EditEVVM : EditPageVMBase<Event>
    {
        readonly ILocationService Geolocation;
        BehaviorSubject<Coordinate> _latest_location = new BehaviorSubject<Coordinate>(Coordinate.Unknown);
        IDisposable _location_subscription = Disposable.Empty;


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
        #endregion

        public EditEVVM(
            ILocationService Geolocation
            )
        {
            Contract.Requires(Geolocation != null);
            this.Geolocation = Geolocation;

            Observable.CombineLatest(
            CurrentModelObservable.Where(m => m.IsNew()),
            ActivationObservable,
            (_, act) => act
            )
                .Subscribe(active =>
                    {
                        if (active)
                        {
                            _latest_location.OnNext(Coordinate.Unknown);
                            _location_subscription = Geolocation.Location().Where(l => !l.IsUnknown()).Subscribe(_latest_location);
                        }
                        else
                        {
                            _location_subscription.Dispose();
                        }
                    });

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

        protected override void UpdateModel()
        {
            if(!Current.Model.IsLocalized())
                Current.Model.SetCoordinates(_latest_location.First());
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
