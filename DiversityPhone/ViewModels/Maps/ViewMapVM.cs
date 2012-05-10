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
using DiversityPhone.Model.Geometry;

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
                ActualPosPoint = this.calculatePercentToPixelPoint(ActualPerc, ActualPosIconSize.X, ActualPosIconSize.Y, Zoom);
                ItemPosPoint = this.calculatePercentToPixelPoint(ItemPerc, ItemPosIconSize.X, ItemPosIconSize.Y, Zoom);
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
                if (ActualPos != null)
                    ActualPerc = this.calculateGPSToPercentagePoint(ActualPos.Latitude, ActualPos.Longitude);
                else
                    ActualPerc = null;
            }
        }

        private Point? _ActualPerc=null;
        public Point? ActualPerc
        {
            get {return _ActualPerc;}
            set
            {
                this.RaiseAndSetIfChanged(x=>x.ActualPerc,ref _ActualPerc,value);
                ActualPosPoint = this.calculatePercentToPixelPoint(ActualPerc, ActualPosIconSize.X, ActualPosIconSize.Y, Zoom);
            }
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

        private ILocalizable _ItemPos=null;
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


        #endregion

        #region Constructor
        public ViewMapVM(IMapStorageService maps,IGeoLocationService geoLoc, ISettingsService settings)            
        {
            Maps = maps;
            Geolocation = geoLoc;
            Settings = settings;
            _SeriesPos = new ObservableCollection<GeoPointForSeries>();
            _SeriesPerc = new ObservableCollection<Point>();
            if(Geolocation.Watcher!=null)
                Geolocation.Watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            Localizable actual = new Localizable();
            Geolocation.fillGeoCoordinates(actual);
            ActualPos = actual;
        }

        #endregion

        #region PointCalculations


        public Point? calculateGPSToPercentagePoint(double? lat, double? lon)
        {
            if (lat == null || Double.IsNaN((double) lat) || lon == null || Double.IsNaN((double) lon)|| Map==null)
                return null;
            return Map.calculatePercentilePositionForMap((double)lat, (double)lon);
        }


        private Point calculatePercentToPixelPoint(Point? percPoint,double iconSizeX,double iconSizeY, double zoom)
        {
            //Check if Point has a Representation on the current map
            if(percPoint==null || percPoint.Value.X<0 || percPoint.Value.X>1 || percPoint.Value.Y<0 || percPoint.Value.Y>1)
                return new Point(-1, -1);
            else
            {
                try
                {
                    int pixelWidth = MapImage.PixelWidth;
                    int pixelHeight = MapImage.PixelHeight;
                    double x = percPoint.Value.X * pixelWidth * zoom - iconSizeX / 2;
                    double y = percPoint.Value.Y * pixelHeight * zoom - iconSizeY / 2;
                    return new Point(x, y);
                }
                catch (Exception e)
                {
                    return new Point(-1, -1);
                }
            }
        }

        public Point calculatePixelPointForSeriesPercPoint(Point? p)
        {
          
           return calculatePercentToPixelPoint(p, SeriesPosIconSize.X, SeriesPosIconSize.Y, Zoom);
           
        }

        private ILocalizable calculatePixelPointToGPS(Point pixelPoint)
        {
            double width=MapImage.PixelWidth*Zoom;
            double height=MapImage.PixelHeight*Zoom;
            double percX = pixelPoint.X / width;
            double percY = pixelPoint.Y / height;
            ILocalizable gpsPoint = Map.calculateGPSFromPerc(percX, percY);
            return gpsPoint;
        }



     

        #endregion


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
                                IList<GeoPointForSeries> geoPoints = Storage.getGeoPointsForSeries(parent); //Binden in AsyncCommand z.B. ViewIU. In AsnyCommand Packen und im Kosntruktor anstoßen+++++++++++++++++++++++                              
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
                        ItemPos = null;
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
