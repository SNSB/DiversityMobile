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
using DiversityPhone.ViewModels;

namespace DiversityPhone.View
{
    
    public class TaxonManagementPivotConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TaxonManagementVM.Pivot pValue;

            try
            {
                pValue = (TaxonManagementVM.Pivot)Enum.Parse(typeof(TaxonManagementVM.Pivot), value.ToString(), true);
            }
            catch (Exception)
            {
                
                return null;
            }

            
            switch (pValue)
	        {
		        case TaxonManagementVM.Pivot.Local:
                    return 0;
                 
                case TaxonManagementVM.Pivot.Repository:
                    return 1;                 
                default:
                    return 0;                 
	        }

            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int iValue;
            try
            {
                iValue = (int)value;
            }
            catch (Exception)
            {
                
                throw;
            }


            switch (iValue)
            {
                case 0:
                    return TaxonManagementVM.Pivot.Local;
                case 1:
                    return TaxonManagementVM.Pivot.Repository;               
                default:
                    System.Diagnostics.Debugger.Break();
                    return TaxonManagementVM.Pivot.Local;                   
            }  
        }
    }
}
