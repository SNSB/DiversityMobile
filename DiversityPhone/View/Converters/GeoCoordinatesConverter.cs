using System;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class GeoCoordinatesConverter : IValueConverter
    {
        public const string LATITUDE = "LAT";
        public const string LONGITUDE = "LON";
        public const string ALTITUDE = "ALT";
        

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is double && parameter != null))
            {                
                return null;
            }

            var dVal = (double)value;            
            switch (parameter.ToString())
            {
                case LATITUDE:
                case LONGITUDE:
                    return LatLonString(dVal, parameter.ToString());
                case ALTITUDE:
                    return AltitudeString(dVal);
                default:
                    return null;
            }

            
        }

        private string DegMinSecString(double value)
        {
            value = Math.Abs(value);
            int deg = (int)Math.Floor(value);
            value -= deg;
            value *= 60;
            int min = (int)Math.Floor(value);
            value -= min;
            value *= 60;
            int sec = (int)Math.Floor(value);

            return string.Format("{0}° {1}' {2}''", deg, min, sec);
        }

        private string AltitudeString(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return DiversityResources.GeoCoordinates_NoAltitude;
            else
                return string.Format("{0} {1}", (int)value, DiversityResources.GeoCoordinates_Suffix_Meter);
        }

        private string LatLonString(double value, string latlon)
        {
            bool isLatitude = latlon == LATITUDE;
            if(double.IsNaN(value) || double.IsInfinity(value))
                return (isLatitude) ? DiversityResources.GeoCoordinates_NoLatitude : DiversityResources.GeoCoordinates_NoLongitude;
            else
                return string.Format("{0} {1}", DegMinSecString(value), GeoSuffix(value, latlon));
        }

        

        private string GeoSuffix(double value, string latlon)
        {
            bool isLatitude = latlon == LATITUDE;
            bool positiveValue = Math.Sign(value) > 0;

            if (isLatitude)
                return (positiveValue) ? DiversityResources.GeoCoordinates_Suffix_N : DiversityResources.GeoCoordinates_Suffix_S;
            else
                return (positiveValue) ? DiversityResources.GeoCoordinates_Suffix_E : DiversityResources.GeoCoordinates_Suffix_W;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }        
    }
}
