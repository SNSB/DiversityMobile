using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DiversityPhone.Model;
using ReactiveUI;
using System.Collections.ObjectModel;
using DiversityPhone.Services;
using System.Device.Location;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels.Maps
{
    public class ViewMapESVM: ViewMapVM
    {
        #region Properties

        private EventSeries _EventSeries;
        public EventSeries EventSeries
        {
            get { return _EventSeries; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.EventSeries, ref _EventSeries, value);
            }
        }

        private bool _EventSeriesIsOpen = false;

        public Point SeriesPosIconSize = new Point(16, 16);

        private ObservableCollection<GeoPointForSeries> _SeriesPos;
        public ObservableCollection<GeoPointForSeries> SeriesPos
        {
            get { return _SeriesPos; }
        }


        // Changes in Values need to be handled on Change-Events in the view
        private ObservableCollection<Point> _SeriesPerc;
        public ObservableCollection<Point> SeriesPerc
        {
            get { return _SeriesPerc; }
        }
        #endregion

        public ViewMapESVM(IMapStorageService maps, IGeoLocationService geoLoc, ISettingsService settings):base(maps,geoLoc,settings)
        {
            _SeriesPos = new ObservableCollection<GeoPointForSeries>();
            _SeriesPerc = new ObservableCollection<Point>();
        }

        public Point calculatePixelPointForSeriesPercPoint(Point? p)
        {

            return calculatePercentToPixelPoint(p, SeriesPosIconSize.X, SeriesPosIconSize.Y, Zoom);

        }


        protected override void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (Geolocation.Watcher == null)
                return;
            if (e.Position != null && e.Position.Location != null)
            {
                Localizable loc = new Localizable();
                loc.Altitude = e.Position.Location.Altitude;
                loc.Latitude = e.Position.Location.Latitude;
                loc.Longitude = e.Position.Location.Longitude;
                ActualPos = loc;
                if (EventSeries != null && _EventSeriesIsOpen)
                {
                    if (SeriesPos != null)
                    {
                        GeoPointForSeries gp = new GeoPointForSeries();
                        gp.Altitude = e.Position.Location.Altitude;
                        gp.Latitude = e.Position.Location.Latitude;
                        gp.Longitude = e.Position.Location.Longitude;
                        SeriesPos.Add(gp);
                    }
                }
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

                if (s.ReferrerType.Equals(ReferrerType.EventSeries))
                {
                    int parent;
                    if (int.TryParse(s.Referrer, out parent))
                    {
                        EventSeries = Storage.getEventSeriesByID(parent);
                        if (EventSeries != null) //Error or NoEventSeries
                        {
                            int? openID = Settings.getSettings().CurrentSeriesID;
                            if (EventSeries.SeriesID == openID)
                                _EventSeriesIsOpen = true;
                            IList<GeoPointForSeries> geoPoints = Storage.getGeoPointsForSeries(parent); //Binden in AsyncCommand z.B. ViewIU. In AsnyCommand Packen und im Kosntruktor anstoßen+++++++++++++++++++++++                              
                            foreach (GeoPointForSeries gp in geoPoints)
                                SeriesPos.Add(gp);
                        }

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

    }
}
