
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
    public class ViewMapIUVM : ViewMapVM
    {

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        public ReactiveCommand Reset { get; private set; }
        #endregion

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

        public Point IUPosIconSize = new Point(32, 32);

        private ILocalizable _IUPos = new Localizable();
        public ILocalizable IUPos
        {
            get { return _IUPos; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.IUPos, ref _IUPos, value);
                if (IUPos != null)
                    IUPerc = this.calculateGPSToPercentagePoint(IUPos.Latitude, IUPos.Longitude);
                else
                    IUPerc = null;
            }
        }

        private Point? _IUPerc = null;
        public Point? IUPerc
        {
            get { return _IUPerc; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.IUPerc, ref _IUPerc, value);
                IUPosPoint = this.calculatePercentToPixelPoint(IUPerc, IUPosIconSize.X, IUPosIconSize.Y, Zoom);
            }
        }

        private Point _IUPosPoint;
        public Point IUPosPoint
        {

            get { return _IUPosPoint; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.IUPosPoint, ref _IUPosPoint, value);
            }
        }

       
        public override double Zoom
        {
            get { return _Zoom; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Zoom, ref _Zoom, value);
                ActualPosPoint = this.calculatePercentToPixelPoint(ActualPerc, ActualPosIconSize.X, ActualPosIconSize.Y, Zoom);
                IUPosPoint = this.calculatePercentToPixelPoint(IUPerc, IUPosIconSize.X, IUPosIconSize.Y, Zoom);
            }
        }

        private ObservableAsPropertyHelper<bool> _IsEditable;
        /// <summary>
        /// Shows, whether the current Object can be Edited
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return _IsEditable.Value;
            }
        }


        #endregion

        public ViewMapIUVM(IMapStorageService maps, IGeoLocationService geoLoc, ISettingsService settings):base(maps,geoLoc,settings)
        {
            
            Save = new ReactiveCommand();
            Delete = new ReactiveCommand();
            Delete.Subscribe(_ => deleteGeoInformation());
            Reset = new ReactiveCommand();
            Reset.Subscribe(_ => restoreGeoInformation());
            //Only IUs that are not unmodified (havent been uploaded) can be made editable
            ToggleEditable = new ReactiveCommand( 
                ValidModel //At this point the UI has been loaded
                .Select(_ => IU)
                .Where(iu => iu != null) // Just to be safe
                .Select(iu => !iu.IsUnmodified())
                .StartWith(false)            
                );
            _IsEditable = ToggleEditable
                .Select(_ => true) 
                .StartWith(false)
                .ToProperty(this, x => x.IsEditable);

                
          

            Messenger.RegisterMessageSource(
               Save
               .Where(_ => IU != null)
               .Do(_ => UpdateModel())
               .Do(_ => OnSave())
               .Select(_ => IU),
               MessageContracts.SAVE);

            //On  Save, Navigate Back
            Messenger.RegisterMessageSource(
               Save
               .Select(_ => Page.Previous)
               );
        }

        private void deleteGeoInformation()
        {
            if (this.IUPos != null)
            {
                Localizable loc = new Localizable();
                this.IUPos = loc;
            }
        }


        private void restoreGeoInformation()
        {
            if (this.IU != null)
            {
                Localizable loc = new Localizable(IU.Altitude, IU.Latitude, IU.Longitude);
                this.IUPos = loc;
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
                        IUPos = new Localizable(IU.Altitude,IU.Latitude,IU.Longitude);                        
                    }
                    else
                    {
                        IUPos = new Localizable();
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

        #region Inheritance
        protected override void UpdateModel()
        {
            if (this.IU != null && this.IUPos!=null)
            {
                this.IU.Altitude = this.IUPos.Altitude;
                this.IU.Latitude = this.IUPos.Latitude;
                this.IU.Longitude = this.IUPos.Longitude;
            }
        }

        protected override void OnSave()
        {
        }

        #endregion
    }
}
