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
using DiversityPhone.ViewModels;

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
                    break;
                case ViewIUVM.Pivots.Analyses:
                    return 1;
                    break;
                case ViewIUVM.Pivots.Descriptions:
                    return 2;
                    break;
                case ViewIUVM.Pivots.Multimedia:
                    return 3;
                    break;
                default:
                    return 0;
                    break;
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
                    return ViewIUVM.Pivots.Analyses;
                case 2:
                    return ViewIUVM.Pivots.Descriptions;
                case 3:
                    return ViewIUVM.Pivots.Multimedia;
                default:
                    System.Diagnostics.Debugger.Break();
                    return ViewIUVM.Pivots.Subunits;
                    break;
            }  
        }
    }
}
