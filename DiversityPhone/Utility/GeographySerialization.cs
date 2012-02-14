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
using System.Text;

namespace DiversityPhone.Utility
{
    public static class GeographySerialization
    {
        public static String serializeGeography(double? latitude, double? longitude, double? altitude)
        {
            String longitudeStr = longitude.ToString();
            longitudeStr = longitudeStr.Replace(',', '.');
            String latStr = latitude.ToString();
            latStr = latStr.Replace(',', '.');
            String altStr = altitude.ToString();
            altStr = altStr.Replace(',', '.');
            StringBuilder builder = new StringBuilder("geography::STGeomFromText('POINT(");
            builder.Append(latStr);
            builder.Append(" ");
            builder.Append(longitudeStr);
            builder.Append(" ");
            builder.Append(altStr);
            builder.Append(")', 4326)");
            String s = builder.ToString();
            return builder.ToString();
        }
    }
}
