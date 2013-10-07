using DiversityPhone.ViewModels;
using System;
using System.Windows.Data;

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
                case ViewEVVM.Pivots.Multimedia:
                 return 2;
                default:
                 System.Diagnostics.Debugger.Break();

                 return 0;
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
                    
            }  
        }
    }
}
