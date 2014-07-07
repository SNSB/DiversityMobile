using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public static class LogFile
    {
        const string LOGFOLDER = "Log";        
        const string LOGFILE_TEMPLATE = "DiversityPhone-{0}.log";
        const int MAX_FILE_COUNT = 31;

        public static ICurrentProfile Profile;

        public static void LogLine(string line)
        {
            using(var store = IsolatedStorageFile.GetUserStoreForApplication())
            using(var fileStream = OpenLogFile(store))
            {
                if(fileStream != Stream.Null)
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine(line);
                        writer.Close();
                    }                    
                }
            }
        }

        private static Stream OpenLogFile(IsolatedStorageFile store)
        {
            if (Profile == null)
            {
                return Stream.Null;
            }

            try
            {
                var fileName = string.Format(LOGFILE_TEMPLATE, DateTime.UtcNow.ToString("yyyy-MM-dd"));
                var folderPath = Path.Combine(Profile.CurrentProfilePath(), LOGFOLDER);

                if (!store.DirectoryExists(folderPath))
                {
                    store.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);

                if (store.FileExists(filePath))
                {
                    return store.OpenFile(filePath, FileMode.Append);
                }
                else
                {
                    CleanupLogs(store, folderPath);

                    return store.OpenFile(filePath, FileMode.Create);
                }
            }
            catch (Exception ex)
            {
                // Logging Failed, don't kill the process because of it
                Debugger.Break();

                return Stream.Null;
            }
        }

        private static void CleanupLogs(IsolatedStorageFile store, string logFolder)
        {
            var files = store.GetFileNames(logFolder);
            var fileCount = files.Count();
            if (fileCount > MAX_FILE_COUNT - 1)
            {
                var filesAgeAsc = from file in files
                                  let age = store.GetCreationTime(file)
                                  orderby age ascending
                                  select file;
                var deleteFiles = filesAgeAsc.Take(fileCount - MAX_FILE_COUNT + 1);

                foreach (var file in deleteFiles)
                {
                    var filePath = Path.Combine(logFolder, file);
                    store.DeleteFile(filePath);
                }                                 
            }
        }
    }
}
