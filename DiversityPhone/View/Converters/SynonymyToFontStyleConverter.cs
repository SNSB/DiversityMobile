using DiversityPhone.Model;
using System;
using System.Windows;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class SynonymyToFontStyleConverter : IValueConverter
    {
        public SynonymyToFontStyleConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value.GetType().ToString().Contains(typeof(Synonymy).ToString()))) //Design Time Data
                throw new NotSupportedException(value.GetType().ToString());

            switch ((Synonymy)Enum.Parse(typeof(Synonymy), value.ToString(), false))
            {
                case Synonymy.Accepted:
                case Synonymy.WorkingName:
                    return FontStyles.Normal;

                case Synonymy.Synonym:
                    return FontStyles.Italic;

                default:
                    return FontStyles.Normal;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}