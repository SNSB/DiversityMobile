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
    public class OldViewMapVM : ElementPageViewModel<Map>
    {

        #region Services

        protected IMapStorageService Maps;
        protected IGeoLocationService Geolocation;
        protected ISettingsService Settings;


        #endregion

        #region Map

        public string Description
        {
            get {

                if (Map != null)
                    return Map.Description;
                else
                    return String.Empty;
            }
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

        
        protected double _Zoom=1;
        public virtual double Zoom
        {
            get { return _Zoom; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Zoom, ref _Zoom, value);
                ActualPosPoint = this.calculatePercentToPixelPoint(ActualPerc, ActualPosIconSize.X, ActualPosIconSize.Y, Zoom);
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

        #region Georef Projection

        public Point ActualPosIconSize = new Point(32, 32);
        
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

        #endregion


        #region Constructor
        public OldViewMapVM(IMapStorageService maps,IGeoLocationService geoLoc, ISettingsService settings)            
        {
            Maps = maps;
            Geolocation = geoLoc;
            Settings = settings;
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
            return Map.PercentilePositionOnMap((double)lat, (double)lon);
        }


        public Point calculatePercentToPixelPoint(Point? percPoint,double iconSizeX,double iconSizeY, double zoom)
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

        public Point calculatePixelToPercentPoint(Point pixelPoint)
        {
            double width = MapImage.PixelWidth * Zoom;
            double height = MapImage.PixelHeight * Zoom;
            double percX = pixelPoint.X / width;
            double percY = pixelPoint.Y / height;
            return new Point(percX, percY);
        }
       

        #endregion


        # region Events

        protected virtual void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
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
            }
        }

        #endregion

        #region MapImage

        protected BitmapImage LoadImage(String uri)
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

        #endregion

        #region Inherited

 

        protected override Map ModelFromState(Services.PageState s)
        {
           
            if (s.Context != null)
            {       
                try
                {
                    //Map = Maps.getMapbyServerKey(s.Context);
                    if (Map != null)
                    {
                        //MapImage= LoadImage(Map.Uri);
                        BaseHeight = MapImage.PixelHeight;
                        BaseWidth = MapImage.PixelWidth;
                        if (ActualPos != null)
                            ActualPerc = calculateGPSToPercentagePoint(ActualPos.Latitude, ActualPos.Longitude);
                        else
                            ActualPerc = null;
                    }
                    else
                        throw new Exception("Map not found");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    Map = null;
                }
                
                return Map;
            }
            return null;
        }

        public override void SaveState()
        {
            base.SaveState();

        }


        protected override ElementVMBase<Map> ViewModelFromModel(Map model)
        {
            return new MapVM(model);
        }

      

        #endregion

        #region IDispose

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


        ~OldViewMapVM()
        {
            try
            {
                this.Dispose();
            }
            catch (Exception) { }
        }

        #endregion

        
    }

}
