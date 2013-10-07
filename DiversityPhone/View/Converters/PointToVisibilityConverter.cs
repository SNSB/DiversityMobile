using System;
using System.Windows;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class PointToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is Point) && targetType == typeof(Visibility)))
                throw new NotSupportedException();

            Point p = (Point)value;
            if (p == null || p.X < 0 || p.Y < 0)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
