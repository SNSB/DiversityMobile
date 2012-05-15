
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

            ToggleEditable = new ReactiveCommand();
            var fals=Observable.Return<bool>(false);
            _IsEditable = new ObservableAsPropertyHelper<bool>(fals, null);
          

            Messenger.RegisterMessageSource(
               Save
               .Where(_ => IU != null)
               .Do(_ => UpdateModel())
               .Do(_ => OnSave())
               .Select(_ => IU),
               MessageContracts.SAVE);

            Messenger.RegisterMessageSource( 
               Delete
               .Where(_ => IU!= null)
               .Select(_ => IU)               
               .Do(_ => OnDelete()),
               MessageContracts.SAVE); //Deletes the geographic Information from the IU and saves the IU after that. The IU itself must not get deleted.

            //On Delete or Save, Navigate Back
            Messenger.RegisterMessageSource(
               Save
               .Merge(Delete)
               .Select(_ => Page.Previous)
               );
        }

        private void deleteGeoInformation()
        {
            if (this.IU != null)
            {
                this.IU.Latitude = null;
                this.IU.Longitude = null;
                this.IU.Altitude = null;
                this.IUPos = this.IU;
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
                        IUPos = IU;
                        var unmodified = Observable.Return<bool>(IU.IsUnmodified());
                        ToggleEditable = new ReactiveCommand(unmodified);
                        _IsEditable = ;
                    }
                    else
                    {
                        IUPos = null;
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
        private override void UpdateModel()
        {

        }

        private override void OnSave()
        {
        }

        private override void OnDelete()
        {
        }

        #endregion
    }
}
