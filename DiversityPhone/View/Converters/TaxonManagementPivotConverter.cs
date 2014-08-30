using DiversityPhone.ViewModels;
using System;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class TaxonManagementPivotConverter : IValueConverter
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

                case TaxonManagementVM.Pivot.Personal:
                    return 1;

                case TaxonManagementVM.Pivot.Public:
                    return 2;

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
                    return TaxonManagementVM.Pivot.Personal;

                case 2:
                    return TaxonManagementVM.Pivot.Public;

                default:
                    System.Diagnostics.Debugger.Break();
                    return TaxonManagementVM.Pivot.Local;
            }
        }
    }
}