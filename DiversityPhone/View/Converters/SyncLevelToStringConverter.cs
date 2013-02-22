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
using DiversityPhone.ViewModels.Utility;

namespace DiversityPhone.View
{
    //[ValueConversion(typeof(bool), typeof(Visibility))]
    public class SyncLevelToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is UploadVM.SyncLevel) && targetType == typeof(String)))
                throw new NotSupportedException();
            var enVal = (UploadVM.SyncLevel)value;

            switch (enVal)
            {
                case UploadVM.SyncLevel.All:
                    return DiversityResources.Sync_Level_All;
                case UploadVM.SyncLevel.EventSeries:
                    return DiversityResources.Sync_Level_EventSeries;
                case UploadVM.SyncLevel.Event:
                    return DiversityResources.Sync_Level_Event;
                case UploadVM.SyncLevel.Specimen:
                    return DiversityResources.Sync_Level_Specimen;
                case UploadVM.SyncLevel.IdentificationUnit:
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
