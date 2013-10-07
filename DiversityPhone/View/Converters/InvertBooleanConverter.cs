using System;
using System.Windows.Data;

namespace DiversityPhone.View
{
    //[ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertBooleanConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
#if DEBUG
            if (!((value is bool) && targetType == typeof(bool)))
                throw new NotSupportedException();
#endif

           
            
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
