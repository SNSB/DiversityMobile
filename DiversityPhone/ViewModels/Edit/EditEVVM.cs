using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using Funq;
using System.Reactive.Subjects;
using System.Device.Location;
using System.Reactive.Disposables;

namespace DiversityPhone.ViewModels
{
    public class EditEVVM : EditPageVMBase<Event>
    {
        ILocationService Geolocation;
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

        private ObservableAsPropertyHelper<string> _CollectionDate;
        public string CollectionDate
        {
            get
            {
                return _CollectionDate.Value;
            }
        }
        #endregion

        public EditEVVM(Container ioc)
        {
            Geolocation = ioc.Resolve<ILocationService>();

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

            _CollectionDate = this.ObservableToProperty(
                CurrentModelObservable
                .Select(ev => ev.CollectionDate.ToString()),
                vm => vm.CollectionDate);

            ModelByVisitObservable
                .Select(ev => ev.LocalityDescription)
                .BindTo(this, x => x.LocalityDescription);

            ModelByVisitObservable
                .Select(ev => ev.HabitatDescription)
                .BindTo(this, x => x.HabitatDescription);

            CanSave().Subscribe(CanSaveSubject.OnNext);
        }

        protected override void UpdateModel()
        {
            if(!Current.Model.IsLocalized())
                Current.Model.SetCoordinates(_latest_location.First());
            Current.Model.LocalityDescription = LocalityDescription;
            Current.Model.HabitatDescription = HabitatDescription;
        }

        protected IObservable<bool> CanSave()
        {
            return this.ObservableForProperty(x => x.LocalityDescription)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);
        }
    }
}
