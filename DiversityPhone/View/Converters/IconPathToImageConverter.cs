using System;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class IconPathToImageConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is string)
                return value;
            else
                throw new NotSupportedException(value.GetType().ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}