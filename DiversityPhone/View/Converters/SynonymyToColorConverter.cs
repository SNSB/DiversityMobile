using System;
using System.Windows.Data;
using DiversityPhone.ViewModels;
using System.Windows.Media;
using DiversityPhone.Model;
using System.Windows;
using System.Linq;

namespace DiversityPhone.View
{

    public class SynonymyToColorConverter : IValueConverter
    {
        private const string ACCEPTED_KEY = "SynonymyAcceptedBrush";
        private const string SYNONYM_KEY = "SynonymySynonymBrush";
        private const string WORKING_KEY = "SynonymyWorkingNameBrush";

        ResourceDictionary _Dictionary;
        ResourceDictionary Dictionary
        {
            get
            {
                return _Dictionary ?? (_Dictionary = findDictionary());
            }
        }
        

        private ResourceDictionary findDictionary()
        {
            var findinmerged = from d in App.Current.Resources.MergedDictionaries
                                where d.Contains(ACCEPTED_KEY)
                                select d;

            if (App.Current.Resources.Contains(ACCEPTED_KEY))
                return App.Current.Resources;
            else
                return findinmerged.FirstOrDefault();
        } 

        public SynonymyToColorConverter()
        {
            
                
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || Dictionary == null)
                return new SolidColorBrush(Colors.White);
            
            if (!(value.GetType().ToString().Contains(typeof(Synonymy).ToString()))) //Design Time Data
                throw new NotSupportedException(value.GetType().ToString());
            
            switch ((Synonymy)Enum.Parse(typeof(Synonymy),value.ToString(), false))
            {
                case Synonymy.Accepted:
                    return Dictionary[ACCEPTED_KEY];
                case Synonymy.Synonym:
                    return Dictionary[SYNONYM_KEY];
                case Synonymy.WorkingName:
                    return Dictionary[WORKING_KEY];
                default:
                    return Dictionary[WORKING_KEY];
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }   
    }
}
