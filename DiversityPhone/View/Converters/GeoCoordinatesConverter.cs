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
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class GeoCoordinatesConverter : IValueConverter
    {
        public const string LATITUDE = "LAT";
        public const string LONGITUDE = "LON";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            
            if (!(value is double))
            {
                System.Diagnostics.Debugger.Break();
                return null;
            }

            var dVal = (double)value;

            return string.Format("{0} {1}", DegMinSecString(dVal), GeoSuffix(dVal, parameter.ToString()));
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

        private string GeoSuffix(double value, string latlon)
        {
            bool isLatitude = latlon == LATITUDE;
            bool positiveValue = Math.Sign(value) > 0;

            if (isLatitude)
                return (positiveValue) ? DiversityResources.GeoCoordinates_Suffix_N : DiversityResources.GeoCoordinates_Suffix_S;
            else
                return (positiveValue) ? DiversityResources.GeoCoordinates_Suffix_W : DiversityResources.GeoCoordinates_Suffix_E;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }        
    }
}
