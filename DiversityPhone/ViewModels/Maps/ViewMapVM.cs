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

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : EditElementPageVMBase<Map>
    {

        #region Services

        IMapStorageService Maps;

        #endregion

        #region Properties


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


        private IGeoLocationService Geolocation;   
        private ILocalizable _ActualPos=new Localizable();
        public ILocalizable ActualPos
        {
            get { return _ActualPos; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ActualPos, ref _ActualPos, value);
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

        

        #endregion


        #endregion


        public ViewMapVM(IMapStorageService maps,IGeoLocationService geoLoc)            
        {
            Maps = maps;
            Geolocation = geoLoc;
            Geolocation.fillGeoCoordinates(ActualPos);
        }

        private Point calculatePixelPoint(double? lat, double? lon)
        {
            if (Map.isOnMap(Map, lat, lon))
            {
                Point p = new Point();
                int pixelWidth = MapImage.PixelWidth;
                int pixelHeight = MapImage.PixelHeight;
                double geoWidth = Math.Abs(Map.LongitudeEast - Map.LongitudeWest);
                double geoHeight = Math.Abs(Map.LatitudeNorth - Map.LatitudeSouth);
                p.X = ((double)lon - Map.LongitudeWest) * pixelWidth / geoWidth * Zoom;
                p.Y = (Map.LatitudeNorth - (double)lat) * pixelHeight / geoHeight * Zoom;
                return p;
            }
            else
                return new Point(-1, -1);
        }


        public void calculatePixelPointForActual()
        {
            if (ActualPos != null)
            {
                Point p = calculatePixelPoint(ActualPos.Latitude, ActualPos.Longitude);
                p.X = p.X - ActualPosIconSize.X / 2;
                p.Y = p.Y - ActualPosIconSize.Y / 2;
                ActualPosPoint = p;
            }
            else ActualPosPoint = new Point(-1, -1);
        }

        public void calculatePixelPointForItem()
        {
            if (ItemPos != null)
            {
                Point p = calculatePixelPoint(ItemPos.Latitude, ItemPos.Longitude);
                p.X = p.X - ItemPosIconSize.X / 2;
                p.Y = p.Y - ItemPosIconSize.Y / 2;
                ItemPosPoint = p;
            }
            else ItemPosPoint = new Point(-1, -1);
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
                                ItemPos = new Localizable();
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

    }

  
    
}
