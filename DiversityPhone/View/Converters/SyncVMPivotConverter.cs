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
using DiversityPhone.ViewModels.Utility;

namespace DiversityPhone.View
{
    
    public class SyncVMPivotConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is SyncVM.Pivots) && targetType == typeof(int)))
                throw new NotSupportedException();


            var v = (SyncVM.Pivots)value;
            switch (v)
            {
                case SyncVM.Pivots.data:
                    return 0;                    
                case SyncVM.Pivots.multimedia:
                    return 1;
                default:
                    System.Diagnostics.Debugger.Break();
                    return 0;                    
            }

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is int) && targetType == typeof(SyncVM.Pivots)))
                throw new NotSupportedException();


            var v = (int)value;
            switch (v)
            {
                case 0:
                    return SyncVM.Pivots.data;
                case 1:
                    return SyncVM.Pivots.multimedia;             
                default:
                    System.Diagnostics.Debugger.Break();
                    return SyncVM.Pivots.data;                    
            }  
        }
    }
}
