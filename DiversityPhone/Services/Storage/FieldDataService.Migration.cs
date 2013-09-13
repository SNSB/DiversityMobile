using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Data.Linq;
using System.Windows;
using System.IO.IsolatedStorage;

namespace DiversityPhone.Services
{
    public partial class FieldDataService
    {
        private const int CURRENT_SCHEMA_VERSION = 0;

        private void ApplyMigrations(DatabaseSchemaUpdater schema)
        {
            // In the future
        }

        public static IEnumerable<string> BackupDBFiles()
        {
            for (int i = 0; i < Int32.MaxValue; i++)
            {
                yield return string.Format("{0}.{1}.bak", DiversityDataContext.DB_FILENAME, i);
            }
            
        }

        public void MoveAndRecreateDatabase()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string bakFileName = BackupDBFiles().First(file => !store.FileExists(file));
               

                store.MoveFile(DiversityDataContext.DB_FILENAME, bakFileName);
                CheckAndRepairDatabase();
            }
        }

        public void CheckAndRepairDatabase()
        {
            bool MoveAndRecreateDB = false;
            withDataContext(ctx =>
            {
                if (!ctx.DatabaseExists())
                {
                    ctx.CreateDatabase();
                    var schema = ctx.CreateDatabaseSchemaUpdater();
                    schema.DatabaseSchemaVersion = CURRENT_SCHEMA_VERSION;
                    schema.Execute();
                }
                else
                {
                    try
                    {
                        var schema = ctx.CreateDatabaseSchemaUpdater();
                        if (schema.DatabaseSchemaVersion != CURRENT_SCHEMA_VERSION)
                        {
                            ApplyMigrations(schema);
                            schema.Execute();
                        }
                    }
                    catch (Exception)
                    {
                        MoveAndRecreateDB = true;
                    }
                }
            });
            if (MoveAndRecreateDB)
            {
                MoveAndRecreateDatabase();
            }
        }

    }
}
