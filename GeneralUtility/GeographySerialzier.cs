using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GlobalUtility
{
    public static class GeographySerialzier
    {
        public static String SerializeGeography(double latitude, double longitude, double? altitude)
        {
            String longitudeStr = longitude.ToString();
            longitudeStr = longitudeStr.Replace(',', '.');
            String latStr = latitude.ToString();
            latStr = latStr.Replace(',', '.');

            StringBuilder builder = new StringBuilder("geography::STGeomFromText('POINT(");
            builder.Append(latStr);
            builder.Append(" ");
            builder.Append(longitudeStr);
            if (altitude != null)
            {
                String altStr = altitude.ToString();
                altStr = altStr.Replace(',', '.');
                builder.Append(" ");
                builder.Append(altStr);
            }
            builder.Append(")', 4326)");
            String s = builder.ToString();
            return builder.ToString();
        }
    }
}
