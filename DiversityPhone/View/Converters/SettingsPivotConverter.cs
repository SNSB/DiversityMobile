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
    
    public class SettingsPivotConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is SetupVM.Pivots) && targetType == typeof(int)))
                throw new NotSupportedException();


            var v = (SetupVM.Pivots)value;
            switch (v)
            {
                case SetupVM.Pivots.Login:
                    return 0;
                case SetupVM.Pivots.Repository:
                    return 1;
                case SetupVM.Pivots.Projects:
                    return 2;
                default:
                    return 0;
            }

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is int) && targetType == typeof(SetupVM.Pivots)))
                throw new NotSupportedException();


            var v = (int)value;
            switch (v)
            {
                case 0:
                    return SetupVM.Pivots.Login;
                case 1:
                    return SetupVM.Pivots.Repository;
                case 2:
                    return SetupVM.Pivots.Projects; 
                default:
                    return SetupVM.Pivots.Login;
            }  
        }
    }
}
