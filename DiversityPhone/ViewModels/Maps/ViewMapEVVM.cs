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
    public class ViewMapEVVM : ViewMapVM
    {

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        public ReactiveCommand Reset { get; private set; }
        #endregion

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

        public Point EVPosIconSize = new Point(32, 32);

        private ILocalizable _EVPos = new Localizable();
        public ILocalizable EVPos
        {
            get { return _EVPos; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.EVPos, ref _EVPos, value);
                if (EVPos != null)
                    EVPerc = this.calculateGPSToPercentagePoint(EVPos.Latitude, EVPos.Longitude);
                else
                    EVPerc = null;
            }
        }

        private Point? _EVPerc = null;
        public Point? EVPerc
        {
            get { return _EVPerc; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.EVPerc, ref _EVPerc, value);
                EVPosPoint = base.calculatePercentToPixelPoint(EVPerc, EVPosIconSize.X, EVPosIconSize.Y, Zoom);
            }
        }

        private Point _EVPosPoint;
        public Point EVPosPoint
        {

            get { return _EVPosPoint; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.EVPosPoint, ref _EVPosPoint, value);
            }
        }

        public override double Zoom
        {
            get { return _Zoom; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Zoom, ref _Zoom, value);
                ActualPosPoint = this.calculatePercentToPixelPoint(ActualPerc, ActualPosIconSize.X, ActualPosIconSize.Y, Zoom);
                EVPosPoint = this.calculatePercentToPixelPoint(EVPerc, EVPosIconSize.X, EVPosIconSize.Y, Zoom);
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

        public ViewMapEVVM(IMapStorageService maps, IGeoLocationService geoLoc, ISettingsService settings):base(maps,geoLoc,settings)
        {
            Save = new ReactiveCommand();
            Delete = new ReactiveCommand();
            Reset = new ReactiveCommand();
            Reset.Subscribe(_ => restoreGeoInformation());
            ToggleEditable = new ReactiveCommand();

            _IsEditable = DistinctStateObservable
               .Select(s => s.Context == null) //Newly created Units are immediately editable //Not necessary here
               .Merge(
                   ToggleEditable.Select(_ => !IsEditable) //Toggle Editable
               )
               .ToProperty(this, vm => vm.IsEditable);

            Messenger.RegisterMessageSource(
               Save
               .Where(_ => Event != null)
               .Do(_ => UpdateModel())
               .Do(_ => OnSave())
               .Select(_ => Event),
               MessageContracts.SAVE);

           

            //On Delete or Save, Navigate Back
            Messenger.RegisterMessageSource(
               Save
               .Select(_ => Page.Previous)
               );
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
                        EVPos = new Localizable(Event.Altitude, Event.Latitude, Event.Longitude);                
                    }
                    else
                        EVPos = null;
                }

                return Map;
            }
            return null;
        }

        private void deleteGeoInformation()
        {
            if (this.EVPos != null)
            {
                Localizable loc = new Localizable();
                this.EVPos = loc;
            }
        }


        private void restoreGeoInformation()
        {
            if (this.Event != null)
            {
                Localizable loc = new Localizable(Event.Altitude, Event.Latitude, Event.Longitude);
                this.EVPos = loc;
            }
        }

        #region Inheritance
        protected override void UpdateModel()
        {

            if (this.Event != null && this.EVPos != null)
            {
                this.Event.Altitude = this.EVPos.Altitude;
                this.Event.Latitude = this.EVPos.Latitude;
                this.Event.Longitude = this.EVPos.Longitude;
            }
        }

        protected override void OnSave()
        {
        }

        protected override void OnDelete()
        {
        }

        #endregion

    }
}
