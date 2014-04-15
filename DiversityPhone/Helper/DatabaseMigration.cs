using DiversityPhone.Services;
using Microsoft.Phone.Data.Linq;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using Ninject;
using DiversityPhone.Model;
using System.IO;
using System.Threading.Tasks;

namespace DiversityPhone.Helper
{
    static class DatabaseMigration
    {        
        private const int CURRENT_SCHEMA_VERSION = 1; // As of Version 0.9.9.1

        public static void CheckAndRepairDatabase()
        {
            bool MoveAndRecreateDB = false;
            using (var ctx = new DiversityDataContext())
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
                            schema.DatabaseSchemaVersion = CURRENT_SCHEMA_VERSION;
                            schema.Execute();
                        }
                    }
                    catch (Exception)
                    {
                        MoveAndRecreateDB = true;
                    }
                }
            }

            if (MoveAndRecreateDB)
            {
                MoveAndRecreateDatabase();
            }
        }

        private static async Task MoveAndRecreateDatabase()
        {
            var backup = App.Kernel.Get<IBackupService>();
            var profile = App.Kernel.Get<ICurrentProfile>();

            await backup.TakeSnapshot();
        }

        private static void ApplyMigrations(DatabaseSchemaUpdater schema)
        {
            // Schema 0 is the default
            // This could be any Version from 0 up to 0.9.9 inclusive
            if (schema.DatabaseSchemaVersion == 0)
            {
                AddMultimediaTimeStamp(schema);                
            }            
        }

        /// <summary>
        /// This Method adds the TimeStamp Column for MMOs, if it doesn't exist
        /// Change made in Commit 22a87e3bfd878951116798ae68ff81b5dd6fa26d
        /// Version 0.9.8
        /// </summary>
        /// <param name="schema"></param>
        private static void AddMultimediaTimeStamp(DatabaseSchemaUpdater schema) {
            try
            {
                var q = from mmo in schema.Context.GetTable<MultimediaObject>()
                        select mmo.TimeStamp;

                if (q.Any())
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                schema.AddColumn<MultimediaObject>("TimeStamp");
            }
        }
    }
}
