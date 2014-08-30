using System;

namespace DiversityPhone.Model
{
    public static class LocalizableMixin
    {
        public static bool IsLocalized(this ILocalizable This)
        {
            return This.Latitude.HasValue && This.Longitude.HasValue && This.Altitude.HasValue;
        }

        public static void SetCoordinates(this ILocalizable This, Coordinate coords)
        {
            if (This == null)
                throw new ArgumentNullException("This");

            if (coords == null)
                throw new ArgumentNullException("coords");

            if (coords.IsUnknown())
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