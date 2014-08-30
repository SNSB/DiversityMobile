using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace DiversityPhone.Helper
{
    internal static class Maintenance
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