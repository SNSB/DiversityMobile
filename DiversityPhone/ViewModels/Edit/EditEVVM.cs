using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class EditEVVM : EditElementPageVMBase<Event>
    {  
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

        public EditEVVM()
        {           
            _CollectionDate = ValidModel
                .Select(ev => ev.CollectionDate.ToString())
                .ToProperty<EditEVVM,string>(this, vm => vm.CollectionDate);

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
                if (s.Referrer != null && int.TryParse(s.Referrer, out parent))
                {
                    return new Event()
                    {
                        SeriesID = parent
                    };
                }
                else
                {
                    return new Event()
                    {
                        SeriesID = null
                    };
                }
            }
            return null;
        }

        protected override ElementVMBase<Event> ViewModelFromModel(Event model)
        {
            return new EventVM(Messenger, model, Page.Current);
        }
    }
}
