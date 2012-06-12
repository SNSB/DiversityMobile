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
    public class EditEVVM : EditElementPageVMBase<Event>
    {
        IGeoLocationService Geolocation;

        #region Properties
        private string _LocalityDescription;
        public string LocalityDescription
        {
            get { return _LocalityDescription; }
            set { this.RaiseAndSetIfChanged(x=>x.LocalityDescription, ref _LocalityDescription, value); }
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
                return  _CollectionDate.Value;
            }
        }
        #endregion

        public EditEVVM(Container ioc)
        {
            Geolocation = ioc.Resolve<IGeoLocationService>();

            _CollectionDate = this.ObservableToProperty(
                ValidModel
                .Select(ev => ev.CollectionDate.ToString()),
                vm => vm.CollectionDate);

            ValidModel
                .Select(ev => ev.LocalityDescription)
                .BindTo(this, x => x.LocalityDescription);

            ValidModel
                .Select(ev => ev.HabitatDescription)
                .BindTo(this, x => x.HabitatDescription);
            
        }   
        
        protected override void UpdateModel()
        {
            Current.Model.LocalityDescription = LocalityDescription;
            Current.Model.HabitatDescription = HabitatDescription;            
        }
       
        protected override IObservable<bool> CanSave()
        {
            return this.ObservableForProperty(x => x.LocalityDescription)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);
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
            else if(s.ReferrerType == ReferrerType.EventSeries)
            {
                int parent;
                Event res;
                if (s.Referrer != null && int.TryParse(s.Referrer, out parent))
                {
                    res = new Event()
                    {
                        SeriesID = parent
                    };
                }
                else
                {
                    res = new Event()
                    {
                        SeriesID = null
                    };
                }
                Geolocation.fillGeoCoordinates(res);
                return res;
            }
            return null;
        }

        protected override ElementVMBase<Event> ViewModelFromModel(Event model)
        {
            return new EventVM(model);
        }
    }
}
