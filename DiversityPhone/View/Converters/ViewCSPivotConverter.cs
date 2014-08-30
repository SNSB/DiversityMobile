using DiversityPhone.ViewModels;
using System;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class ViewCSPivotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is ViewCSVM.Pivots) && targetType == typeof(int)))
                throw new NotSupportedException();

            var v = (ViewCSVM.Pivots)value;
            switch (v)
            {
                case ViewCSVM.Pivots.Units:
                    return 0;

                case ViewCSVM.Pivots.Multimedia:
                    return 1;

                default:
                    System.Diagnostics.Debugger.Break();

                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is int) && targetType == typeof(ViewCSVM.Pivots)))
                throw new NotSupportedException();

            var v = (int)value;
            switch (v)
            {
                case 0:
                    return ViewCSVM.Pivots.Units;

                case 1:
                    return ViewCSVM.Pivots.Multimedia;

                default:
                    System.Diagnostics.Debugger.Break();
                    return ViewCSVM.Pivots.Units;
            }
        }
    }
}