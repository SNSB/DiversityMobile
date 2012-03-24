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

    public class ViewEVPivotConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is ViewEVVM.Pivots) && targetType == typeof(int)))
                throw new NotSupportedException();

            
            var v = (ViewEVVM.Pivots)value;
            switch (v)
	        {
		        case ViewEVVM.Pivots.Specimen:
                 return 0;                 
                case ViewEVVM.Pivots.Descriptions:
                 return 1;
                 break;
                case ViewEVVM.Pivots.Multimedia:
                 return 2;
                 break;
                default:
                 System.Diagnostics.Debugger.Break();

                 return 0;
                 break;
	        }  

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is int))
                throw new NotSupportedException();


            var v = (int)value;
            switch (v)
            {
                case 0:
                    return ViewEVVM.Pivots.Specimen;
                case 1:
                    return ViewEVVM.Pivots.Descriptions;
                case 2:
                    return ViewEVVM.Pivots.Multimedia;
                default:
                    System.Diagnostics.Debugger.Break();
                    return ViewEVVM.Pivots.Specimen;
                    break;
            }  
        }
    }
}
