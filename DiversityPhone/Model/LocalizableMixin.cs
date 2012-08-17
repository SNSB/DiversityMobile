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

namespace DiversityPhone.Model
{
    public static class LocalizableMixin
    {
        public static bool IsLocalized(this ILocalizable This)
        {
            return This.Latitude.HasValue && This.Longitude.HasValue && This.Altitude.HasValue;
        }

        public static void SetGeoCoordinates(this ILocalizable This, GeoCoordinate coords)
        {
            if (This == null)
                throw new ArgumentNullException("This");

            if (coords == null)
                throw new ArgumentNullException("coords");

            if (coords.IsUnknown)
            {
                This.Altitude = null;
                This.Longitude = null;
                This.Latitude = null;
            }
            else
            {
                This.Altitude = coords.Altitude;
                This.Longitude = coords.Longitude;
                This.Latitude = coords.Latitude;
            }
        }
    }
}
