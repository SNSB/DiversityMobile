
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using DiversityPhone.Messages;
using System.Windows;
using System;

namespace DiversityPhone.ViewModels.Maps
{
    public class ViewMapIUVM : ViewMapEditVM
    {

      
        #region Properties

        private IdentificationUnit _IU;
        public IdentificationUnit IU
        {
            get { return _IU; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.IU, value);
            }
        }


        #endregion

        public ViewMapIUVM(IMapStorageService maps, IGeoLocationService geoLoc, ISettingsService settings):base(maps,geoLoc,settings)
        {
            Reset.Subscribe(_ => restoreGeoInformation());
            //Only IUs that are not unmodified (havent been uploaded) can be made editable
            ToggleEditable = new ReactiveCommand( 
                ValidModel //At this point the UI has been loaded
                .Select(_ => IU)
                .Where(iu => iu != null) // Just to be safe
                .Select(iu => !iu.IsUnmodified())
                .StartWith(false)            
                );
            _IsEditable = this.ObservableToProperty(
                ToggleEditable
                .Select(_ => true) 
                .StartWith(false),
                x => x.IsEditable);

            Messenger.RegisterMessageSource(
               Save
               .Where(_ => IU != null)
               .Do(_ => UpdateModel())
               .Do(_ => OnSave())
               .Select(_ => IU),
               MessageContracts.SAVE);

        }


        private void restoreGeoInformation()
        {
            if (this.IU != null)
            {
                Localizable loc = new Localizable(IU.Altitude, IU.Latitude, IU.Longitude);
                this.ItemPos = loc;
            }
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

                if (s.ReferrerType.Equals(ReferrerType.IdentificationUnit))
                {
                    int parent;
                    if (int.TryParse(s.Referrer, out parent))
                    {
                        IU = Storage.getIdentificationUnitByID(parent);
                        ItemPos = new Localizable(IU.Altitude,IU.Latitude,IU.Longitude);                        
                    }
                    else
                    {
                        ItemPos = new Localizable();
                        IU = null;
                    }
                }
                else
                {
                    //Error
                    throw new Exception("Type Mismatch");
                }
                return Map;
            }
            return null;
        }

 
        protected void UpdateModel()
        {
            if (this.IU != null && this.ItemPos!=null)
            {
                this.IU.Altitude = this.ItemPos.Altitude;
                this.IU.Latitude = this.ItemPos.Latitude;
                this.IU.Longitude = this.ItemPos.Longitude;
            }
        }

        protected void OnSave()
        {
        }

 
    }
}
