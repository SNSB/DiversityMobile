using System;
using System.Net;
using System.Windows;

using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Services;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels.Maps
{
    public class ViewMapEditVM : OldViewMapVM
    {
        #region Commands
        public ReactiveCommand Save { get; protected set; }
        public ReactiveCommand ToggleEditable { get; protected set; }
        public ReactiveCommand Delete { get; protected set; }
        public ReactiveCommand Reset { get; protected set; }

        #endregion

        #region Properties

        public Point ItemPosIconSize = new Point(32, 32);

        private ILocalizable _ItemPos = new Localizable();
        public ILocalizable ItemPos
        {
            get { return _ItemPos; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ItemPos, ref _ItemPos, value);
                if (ItemPos != null)
                    ItemPerc = this.calculateGPSToPercentagePoint(ItemPos.Latitude, ItemPos.Longitude);
                else
                    ItemPerc = null;
            }
        }

        private Point? _ItemPerc = null;
        public Point? ItemPerc
        {
            get { return _ItemPerc; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ItemPerc, ref _ItemPerc, value);
                ItemPosPoint = this.calculatePercentToPixelPoint(ItemPerc, ItemPosIconSize.X, ItemPosIconSize.Y, Zoom);
            }
        }

        private Point _ItemPosPoint;
        public Point ItemPosPoint
        {

            get { return _ItemPosPoint; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ItemPosPoint, ref _ItemPosPoint, value);
            }
        }

       
        public override double Zoom
        {
            get { return _Zoom; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Zoom, ref _Zoom, value);
                ActualPosPoint = this.calculatePercentToPixelPoint(ActualPerc, ActualPosIconSize.X, ActualPosIconSize.Y, Zoom);
                ItemPosPoint = this.calculatePercentToPixelPoint(ItemPerc, ItemPosIconSize.X, ItemPosIconSize.Y, Zoom);
            }
        }

        protected ObservableAsPropertyHelper<bool> _IsEditable;
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

        
        public ViewMapEditVM(IMapStorageService maps, IGeoLocationService geoLoc, ISettingsService settings):base(maps,geoLoc,settings)
        {
            
            Save = new ReactiveCommand();
            Delete = new ReactiveCommand();
            Delete.Subscribe(_ => deleteGeoInformation());
            Reset = new ReactiveCommand();
           
  

            //On  Save, Navigate Back
            Messenger.RegisterMessageSource(
               Save
               .Select(_ => Page.Previous)
               );
        }

        public ILocalizable calculatePixelPointToGPS(Point pixelPoint)
        {
            //Point percPoint = this.calculatePixelToPercentPoint(pixelPoint);
            //ILocalizable gpsPoint = Map.calculateGPSFromPerc(percPoint.X, percPoint.Y);
            //return gpsPoint;
            return null;
        }


        private void deleteGeoInformation()
        {
            if (this.ItemPos != null)
            {
                Localizable loc = new Localizable();
                this.ItemPos = loc;
            }
        }

    }
}
