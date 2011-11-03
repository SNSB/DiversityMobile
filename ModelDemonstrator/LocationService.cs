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
using System.Device.Location;
using System.Threading;

namespace ModelDemonstrator
{
    public class LocationService
    {
        private static LocationService instance;

        private GeoCoordinateWatcher watcher;

        private LocationService() 
        {
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            watcher.MovementThreshold = 20;
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            
            txtBox.Text = Thread.CurrentThread.IsBackground + " " + e.Position.Location.ToString();
        }

        private TextBox txtBox;

        public void Initialize(TextBox box /* ... */)
        {
            /* Parameter abspeichern */

            txtBox = box;
            
        }

        public static LocationService INSTANCE
        {
            get 
            {
                if (instance == null) {
                    instance = new LocationService();
                }
                return instance;
            }
        }

        public void Start()
        {
            // ...
            watcher.Start();
        }

        public void Stop()
        {
            // ...

            watcher.Stop();
        }
    }
}
