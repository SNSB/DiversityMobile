using System;
using System.Globalization;

namespace DiversityPhone
{
    public static class FileTimeStampMixin
    {
        public static string ToFileTimeStamp(this DateTime This)
        {
            return This.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        }
    }
}
