using DiversityPhone.ViewModels;
using System;
using System.Diagnostics;
using System.Windows.Data;

namespace DiversityPhone.View
{
    
    public class ImportExportStateConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is ImportExportVM.Status) && targetType == typeof(string)))
                throw new NotSupportedException();


            var v = (ImportExportVM.Status)value;
            switch (v)
            {
                case ImportExportVM.Status.TakeAppData:
                    return DiversityResources.ImportExport_Status_TakeAppData;
                case ImportExportVM.Status.TakeExternalImages:
                    return DiversityResources.ImportExport_Status_TakeExternal;
                case ImportExportVM.Status.Restore:
                    return DiversityResources.ImportExport_Status_Restore;
                case ImportExportVM.Status.Delete:
                    return DiversityResources.ImportExport_Status_Delete;
                case ImportExportVM.Status.Upload:
                    return DiversityResources.ImportExport_Status_Upload;
                case ImportExportVM.Status.Download:
                    return DiversityResources.ImportExport_Status_Download;
                default:
                    Debugger.Break();
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
