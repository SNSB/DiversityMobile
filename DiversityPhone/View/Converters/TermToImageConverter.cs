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
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Resources;
using System.Linq;

namespace DiversityPhone.View
{
    /// <summary>
    /// Converter Class that takes an image name and computes a Taxon image path from it.
    /// </summary>
    public class TermToImageConverter : IValueConverter
    {
        private const string DEFAULT_IMAGE = "/Images/SNSBIcons/Taxa/other_80.png";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            if (value != null)                
            {
                var computedPath = String.Format("/Images/SNSBIcons/Taxa/{0}_80.png", value.ToString());
                if(CanLoadResource(computedPath))
                    return computedPath;
            }
            return DEFAULT_IMAGE;     
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines, wheter a given Resource path points to a valid Resource
        /// </summary>
        /// <remarks>
        /// Implementation is crude, but it works.
        /// </remarks> 
        /// <param name="uri">resource uri</param>
        /// <returns>Whether or not the Resource exists.</returns>
        private static bool CanLoadResource(string uri)
        {
            try
            {
                Application.GetResourceStream(new Uri(uri, UriKind.RelativeOrAbsolute));
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

    }
}
