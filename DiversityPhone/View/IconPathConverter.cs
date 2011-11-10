using System;
using System.Windows.Data;
using DiversityPhone.ViewModels;
using System.Windows.Media;

namespace DiversityPhone.View
{
    
    public class IconPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            
            if (!(value.GetType().ToString().Contains(typeof(Icon).ToString()))) //Design Time Data
                throw new NotSupportedException(value.GetType().ToString());

            string imageURI = "/Images/appbar.add.rest.png";
            switch ((Icon)Enum.Parse(typeof(Icon),value.ToString(), false))
            {
                case Icon.EventSeries:
                    return "/Images/SNSBIcons/Series_80.png";
                case Icon.NoEventSeries:
                    return "/Images/SNSBIcons/Event_80.png";
                case Icon.Event:
                    return "/Images/SNSBIcons/Event_80.png";
                   
                
                case Icon.Observation:
                    break;
                                  
            }
            if (targetType == typeof(ImageSource))
                return new ImageSourceConverter().ConvertFromString(imageURI);
            else
                return imageURI;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
