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
using ReactiveUI;
using DiversityPhone.Model;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Device.Location;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : EditElementPageVMBase<Map>, IDisposable
    {

        #region Services

        private IMapStorageService Maps;
        private IGeoLocationService Geolocation;
        private ISettingsService Settings;


        #endregion

        #region Properties

        #region Map

        public string Description
        {
            get { return Map.Description; }
        }


     
        private Map _Map;
        public Map Map
        {
            get { return _Map; }
            set { this.RaiseAndSetIfChanged(x => x.Map, ref _Map, value); }
        }


        private BitmapImage _mapImage;

        public BitmapImage MapImage
        {
            get
            {
                return _mapImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapImage, ref _mapImage, value);
            }
        }

        
        private double _Zoom=1;
        public double Zoom
        {
            get { return _Zoom; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Zoom, ref _Zoom, value);
            }
        }

        private double _BaseHeight;
        public double BaseHeight
        {
            get { return _BaseHeight; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.BaseHeight, ref _BaseHeight, value);
            }
        }

        private double _BaseWidth;
        public double BaseWidth
        {
            get { return _BaseWidth; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.BaseWidth, ref _BaseWidth, value);
            }
        }

        #endregion
        #region Referrers

        private EventSeries _EventSeries;
        public EventSeries EventSeries
        {
            get { return _EventSeries; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.EventSeries, ref _EventSeries, value);
            }
        }

        private bool _EventSeriesIsOpen=false;

        private Event _Event;
        public Event Event
        {
            get { return _Event; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Event, ref _Event, value);
            }
        }

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


        #region Georef


        public Point ActualPosIconSize = new Point(32, 32);
        public Point ItemPosIconSize = new Point(32, 32);
        public Point SeriesPosIconSize = new Point(16, 16);


        private ILocalizable _ActualPos=new Localizable();
        public ILocalizable ActualPos
        {
            get { return _ActualPos; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ActualPos, ref _ActualPos, value);
                calculatePixelPointForActual();
            }
        }

        private ILocalizable _ItemPos;
        public ILocalizable ItemPos
        {
            get { return _ItemPos; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ItemPos, ref _ItemPos, value);
                calculatePixelPointForItem();
            }
        }

        private ObservableCollection<GeoPointForSeries> _SeriesPos;
        public ObservableCollection<GeoPointForSeries> SeriesPos
        {
            get { return _SeriesPos; }
        }


        private Point _ActualPosPoint;
        public Point ActualPosPoint
        {
            get
            {
                return _ActualPosPoint;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ActualPosPoint, ref _ActualPosPoint, value);
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

        //private ObservableCollection<Point> _SeriesPosPoints;
        //public ObservableCollection<Point> SeriesPosPoints
        //{
        //    get { return _SeriesPosPoints; }
        //}

        #endregion


        #endregion


        public ViewMapVM(IMapStorageService maps,IGeoLocationService geoLoc, ISettingsService settings)            
        {
            Maps = maps;
            Geolocation = geoLoc;
            Settings = settings;
            _SeriesPos = new ObservableCollection<GeoPointForSeries>();
            if(Geolocation.Watcher!=null)
                Geolocation.Watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            Localizable actual = new Localizable();
            Geolocation.fillGeoCoordinates(actual);
            ActualPos = actual;
        }

        private Point calculatePixelPoint(double? lat, double? lon,double iconSizeX,double iconSizeY, double zoom)
        {
            if (Map.isOnMap(Map, lat, lon))
            {
                Point p = new Point();
                int pixelWidth = MapImage.PixelWidth;
                int pixelHeight = MapImage.PixelHeight;
                double geoWidth = Math.Abs(Map.LongitudeEast - Map.LongitudeWest);
                double geoHeight = Math.Abs(Map.LatitudeNorth - Map.LatitudeSouth);
                p.X = ((double)lon - Map.LongitudeWest) * pixelWidth / geoWidth * zoom - iconSizeX / 2;
                p.Y = (Map.LatitudeNorth - (double)lat) * pixelHeight / geoHeight * zoom - iconSizeY / 2;
                return p;
            }
            else
                return new Point(-1, -1);
        }


        public void calculatePixelPointForActual()
        {
            if (ActualPos != null)
            {
                Point p = calculatePixelPoint(ActualPos.Latitude, ActualPos.Longitude,ActualPosIconSize.X,ActualPosIconSize.Y,Zoom);
                ActualPosPoint = p;
            }
            else ActualPosPoint = new Point(-1, -1);
        }

        public void calculatePixelPointForItem()
        {
            if (ItemPos != null)
            {
                Point p = calculatePixelPoint(ItemPos.Latitude, ItemPos.Longitude, ItemPosIconSize.X,ItemPosIconSize.Y,Zoom);
                ItemPosPoint = p;
            }
            else ItemPosPoint = new Point(-1, -1);
        }


        public Point calculatePixelPointForSeriesPoint(GeoPointForSeries gp)
        {
            if (gp != null)
            {
                return calculatePixelPoint(gp.Latitude, gp.Longitude, SeriesPosIconSize.X, SeriesPosIconSize.Y, Zoom);
            }
            else return new Point(-1, -1);
        }


        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
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


        private BitmapImage LoadImage(String uri)
        {
            byte[] data;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                using (IsolatedStorageFileStream isfs = isf.OpenFile(uri, FileMode.Open, FileAccess.Read))
                {
                    data = new byte[isfs.Length];
                    isfs.Read(data, 0, data.Length);
                    isfs.Close();
                }

            }
            MemoryStream ms = new MemoryStream(data);
            BitmapImage bi = new BitmapImage();
            bi.SetSource(ms);
            return bi;
        }

 

        protected override void UpdateModel()
        {
            
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
                        MapImage= LoadImage(Map.Uri);
                        BaseHeight = MapImage.PixelHeight;
                        BaseWidth = MapImage.PixelWidth;
                        if (ActualPos != null)
                            calculatePixelPointForActual();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    Map = null;
                }

                if (s.ReferrerType!= null)
                {
                    int parent;
                    if (int.TryParse(s.Referrer, out parent))
                    {
                        switch (s.ReferrerType)
                        {
                            case ReferrerType.EventSeries:
                                EventSeries = Storage.getEventSeriesByID(parent);
                                int? openID = Settings.getSettings().CurrentSeriesID;
                                if (EventSeries.SeriesID == openID)
                                    _EventSeriesIsOpen = true;
                                ItemPos = new Localizable();
                                IList<GeoPointForSeries> geoPoints = Storage.getGeoPointsForSeries(parent);
                                
                                foreach (GeoPointForSeries gp in geoPoints)
                                    SeriesPos.Add(gp);
                                break;
                            case ReferrerType.Event:
                                Event = Storage.getEventByID(parent);
                                ItemPos = Event;
                                break;
                            case ReferrerType.IdentificationUnit:
                                IU = Storage.getIdentificationUnitByID(parent);
                                ItemPos = IU;
                                break;
                            case ReferrerType.Specimen:
                            case ReferrerType.None:
                            default:
                                ItemPos = new Localizable();
                                break;
                        }
                    }
                    else
                        ItemPos = new Localizable();
                }

                return Map;
            }
            return null;
        }

        public override void SaveState()
        {
            base.SaveState();

        }


        protected override IObservable<bool> CanSave()
        {
            return Observable.Return(false);
        }

        protected override ElementVMBase<Map> ViewModelFromModel(Map model)
        {
            return new MapVM(Messenger, model, DiversityPhone.Services.Page.Current);
        }

        public void Dispose()
        {
            try
            {
                if (Geolocation.Watcher != null)
                    Geolocation.Watcher.PositionChanged -= new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            }
            catch (Exception e)
            {
            }
        }


        ~ViewMapVM()
        {
            try
            {
                this.Dispose();
            }
            catch (Exception) { }
        }
    }

  
    
}
