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
using DiversityPhone.Model;

namespace DiversityPhone.View
{
    //[ValueConversion(typeof(bool), typeof(Visibility))]
    public class DBTypeToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is DBObjectType) && targetType == typeof(String)))
                throw new NotSupportedException();
            var enVal = (DBObjectType)value;

            switch (enVal)
            {
                case DBObjectType.EventSeries:
                    return DiversityResources.Sync_Level_EventSeries;
                case DBObjectType.Event:
                    return DiversityResources.Sync_Level_Event;
                case DBObjectType.Specimen:
                    return DiversityResources.Sync_Level_Specimen;
                case DBObjectType.IdentificationUnit:
                    return DiversityResources.Sync_Level_IU;
                default:
                    throw new ArgumentException("value");
            }    
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
