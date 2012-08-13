using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using Funq;

namespace DiversityPhone.ViewModels
{
    public class EditEVVM : EditPageVMBase<Event>
    {
        IGeoLocationService Geolocation;

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
            Geolocation = ioc.Resolve<IGeoLocationService>();

            _CollectionDate = this.ObservableToProperty(
                CurrentModelObservable
                .Select(ev => ev.CollectionDate.ToString()),
                vm => vm.CollectionDate);

            CurrentModelObservable
                .Select(ev => ev.LocalityDescription)
                .BindTo(this, x => x.LocalityDescription);

            CurrentModelObservable
                .Select(ev => ev.HabitatDescription)
                .BindTo(this, x => x.HabitatDescription);

            CanSave().Subscribe(CanSaveSubject.OnNext);
        }

        protected override void UpdateModel()
        {
            Geolocation.fillGeoCoordinates(Current.Model);
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
