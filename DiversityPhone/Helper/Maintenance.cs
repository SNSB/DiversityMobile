using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiversityPhone.Helper
{
    static class Maintenance
    {


        public static async Task CleanupTempFolder()
        {
            var TEMP_FOLDER = "Temp";

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.DirectoryExists(TEMP_FOLDER))
                {
                    await iso.DeleteDirectoryRecursiveAsync(TEMP_FOLDER);
                }
                if (!iso.DirectoryExists(TEMP_FOLDER))
                {
                    iso.CreateDirectory(TEMP_FOLDER);
                }
            }
        }
    }
}
