using DiversityPhone.ViewModels;
using System;
using System.Windows.Data;

namespace DiversityPhone.View
{
    public class ImportExportPivotConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is ImportExportVM.Pivot) && targetType == typeof(int)))
                throw new NotSupportedException();


            var v = (ImportExportVM.Pivot)value;
            switch (v)
            {
                case ImportExportVM.Pivot.local:
                    return 0;
                case ImportExportVM.Pivot.remote:
                    return 1;

                default:
                    System.Diagnostics.Debugger.Break();

                    return 0;

            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is int) && targetType == typeof(ImportExportVM.Pivot)))
                throw new NotSupportedException();


            var v = (int)value;
            switch (v)
            {
                case 0:
                    return ImportExportVM.Pivot.local;
                case 1:
                    return ImportExportVM.Pivot.remote;
                default:
                    System.Diagnostics.Debugger.Break();
                    return ImportExportVM.Pivot.local;
            }
        }
    }
}
