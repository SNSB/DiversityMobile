using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiversityPhone.View  
{  
    public class StreamToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            

            var s = value as Stream;

            if(s != null && targetType == typeof(ImageSource))
            {
                var img = new BitmapImage();
                img.SetSource(s);

                return img;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
