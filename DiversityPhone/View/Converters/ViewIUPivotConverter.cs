using DiversityPhone.ViewModels;
using System;
using System.Windows.Data;

namespace DiversityPhone.View
{
    
    public class ViewIUPivotConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is ViewIUVM.Pivots) && targetType == typeof(int)))
                throw new NotSupportedException();


            var v = (ViewIUVM.Pivots)value;
            switch (v)
            {
                case ViewIUVM.Pivots.Subunits:
                    return 0;
                case ViewIUVM.Pivots.Descriptions:
                    return 1;            
                case ViewIUVM.Pivots.Multimedia:
                    return 2;
                default:
                    return 0;
            }

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is int) && targetType == typeof(ViewIUVM.Pivots)))
                throw new NotSupportedException();


            var v = (int)value;
            switch (v)
            {
                case 0:
                    return ViewIUVM.Pivots.Subunits;
                case 1:
                    return ViewIUVM.Pivots.Descriptions;
                case 2:                    
                    return ViewIUVM.Pivots.Multimedia;
                default:
                    System.Diagnostics.Debugger.Break();
                    return ViewIUVM.Pivots.Subunits;                    
            }  
        }
    }
}
