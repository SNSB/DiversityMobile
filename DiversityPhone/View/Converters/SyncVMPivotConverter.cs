namespace DiversityPhone.View
{
    using DiversityPhone.ViewModels;
    using System;
    using System.Windows.Data;

    public class SyncVMPivotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is UploadVM.Pivots) && targetType == typeof(int)))
                throw new NotSupportedException();

            var v = (UploadVM.Pivots)value;
            switch (v)
            {
                case UploadVM.Pivots.data:
                    return 0;

                case UploadVM.Pivots.multimedia:
                    return 1;

                default:
                    System.Diagnostics.Debugger.Break();
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((value is int) && targetType == typeof(UploadVM.Pivots)))
                throw new NotSupportedException();

            var v = (int)value;
            switch (v)
            {
                case 0:
                    return UploadVM.Pivots.data;

                case 1:
                    return UploadVM.Pivots.multimedia;

                default:
                    System.Diagnostics.Debugger.Break();
                    return UploadVM.Pivots.data;
            }
        }
    }
}