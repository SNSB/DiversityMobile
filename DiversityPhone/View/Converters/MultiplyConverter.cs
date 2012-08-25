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
    public class MultiplyConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
#if DEBUG
            if (!((value is double) && targetType == typeof(double)))
                throw new NotSupportedException();
#endif

            var val = value as double?;
            var factor = parameter as double?;
            if (val.HasValue && factor.HasValue)
                return val.Value * factor.Value;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
