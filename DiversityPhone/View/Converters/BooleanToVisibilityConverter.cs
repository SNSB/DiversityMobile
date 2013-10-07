﻿using System;
using System.Windows;
using System.Windows.Data;

namespace DiversityPhone.View
{
    //[ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is bool) && targetType == typeof(Visibility)))
                throw new NotSupportedException();

            bool invert = false;
            if (parameter != null && parameter is string)
            {
                bool.TryParse(parameter as string, out invert);
            }

            return ((bool)value ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
