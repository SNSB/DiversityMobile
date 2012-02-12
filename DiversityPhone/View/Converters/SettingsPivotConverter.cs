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
            if (!((value is SettingsVM.Pivots) && targetType == typeof(int)))
                throw new NotSupportedException();


            var v = (SettingsVM.Pivots)value;
            switch (v)
            {
                case SettingsVM.Pivots.Login:
                    return 0;
                case SettingsVM.Pivots.Repository:
                    return 1;
                case SettingsVM.Pivots.Projects:
                    return 2;
                default:
                    return 0;
            }

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is int) && targetType == typeof(SettingsVM.Pivots)))
                throw new NotSupportedException();


            var v = (int)value;
            switch (v)
            {
                case 0:
                    return SettingsVM.Pivots.Login;
                case 1:
                    return SettingsVM.Pivots.Repository;
                case 2:
                    return SettingsVM.Pivots.Projects; 
                default:
                    return SettingsVM.Pivots.Login;
            }  
        }
    }
}
