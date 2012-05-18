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
    public class ViewMapEditVM : ViewMapVM
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
                    IUPerc = this.calculateGPSToPercentagePoint(ItemPos.Latitude, ItemPos.Longitude);
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
                IUPosPoint = this.calculatePercentToPixelPoint(IUPerc, ItemPosIconSize.X, ItemPosIconSize.Y, Zoom);
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
                IUPosPoint = this.calculatePercentToPixelPoint(IUPerc, ItemPosIconSize.X, ItemPosIconSize.Y, Zoom);
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
            
  
            _IsEditable = ToggleEditable
                .Select(_ => true) 
                .StartWith(false)
                .ToProperty(this, x => x.IsEditable);

            //On  Save, Navigate Back
            Messenger.RegisterMessageSource(
               Save
               .Select(_ => Page.Previous)
               );
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
