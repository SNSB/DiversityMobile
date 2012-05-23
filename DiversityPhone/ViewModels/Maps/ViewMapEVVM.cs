using System;

using System.Windows.Shapes;
using DiversityPhone.Model;

using ReactiveUI;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using DiversityPhone.Messages;
using System.Windows;

namespace DiversityPhone.ViewModels.Maps
{
    public class ViewMapEVVM : ViewMapEditVM
    {

      

        #region Properties

        private Event _Event;
        public Event Event
        {
            get { return _Event; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Event, ref _Event, value);
            }
        }

       

        #endregion

        public ViewMapEVVM(IMapStorageService maps, IGeoLocationService geoLoc, ISettingsService settings):base(maps,geoLoc,settings)
        {
          
            Reset.Subscribe(_ => restoreGeoInformation());

            ToggleEditable = new ReactiveCommand(
              ValidModel //At this point the UI has been loaded
              .Select(_ => Event)
              .Where(ev => ev != null) // Just to be safe
              .Select(ev => !ev.IsUnmodified())
              .StartWith(false)
              );

            _IsEditable = this.ObservableToProperty(
                ToggleEditable
                .Select(_ => true)
                .StartWith(false),
                x => x.IsEditable);

            Messenger.RegisterMessageSource(
               Save
               .Where(_ => Event != null)
               .Do(_ => UpdateModel())
               .Do(_ => OnSave())
               .Select(_ => Event),
               MessageContracts.SAVE);
        }


        protected override Map ModelFromState(Services.PageState s)
        {
            if (s.Context != null)
            {
                try
                {
                    Map = Maps.getMapbyServerKey(s.Context);
                    if (Map != null)
                    {
                        MapImage = LoadImage(Map.Uri);
                        BaseHeight = MapImage.PixelHeight;
                        BaseWidth = MapImage.PixelWidth;
                        if (ActualPos != null)
                            ActualPerc = calculateGPSToPercentagePoint(ActualPos.Latitude, ActualPos.Longitude);
                        else
                            ActualPerc = null;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    Map = null;
                }

                if (s.ReferrerType.Equals(ReferrerType.Event))
                {
                    int parent;
                    if (int.TryParse(s.Referrer, out parent))
                    {
                        Event = Storage.getEventByID(parent);
                        ItemPos = new Localizable(Event.Altitude, Event.Latitude, Event.Longitude);                
                    }
                    else
                        ItemPos = null;
                }

                return Map;
            }
            return null;
        }

       


        private void restoreGeoInformation()
        {
            if (this.Event != null)
            {
                Localizable loc = new Localizable(Event.Altitude, Event.Latitude, Event.Longitude);
                this.ItemPos = loc;
            }
        }

       
        protected void UpdateModel()
        {

            if (this.Event != null && this.ItemPos != null)
            {
                this.Event.Altitude = this.ItemPos.Altitude;
                this.Event.Latitude = this.ItemPos.Latitude;
                this.Event.Longitude = this.ItemPos.Longitude;
            }
        }

        protected  void OnSave()
        {
        }

        protected void OnDelete()
        {
        }

      

    }
}
