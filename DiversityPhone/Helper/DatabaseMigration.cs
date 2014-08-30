namespace DiversityPhone.Helper
{
    using DiversityPhone.Model;
    using DiversityPhone.Services;
    using Microsoft.Phone.Data.Linq;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class VersionMigration
    {
        public static readonly Version CURRENT_VERSION = new Version(0, 9, 9, 1);
        public const int CURRENT_SCHEMA_VERSION = 1; // As of Version 0.9.9.1

        public static Task CreateOrUpgradeSchema(string dbPath = null, Version targetVersion = null)
        {
            return Task.Factory.StartNew(() =>
            {
                var ctx = (dbPath != null) ? new DiversityDataContext(dbPath) : new DiversityDataContext();
                using (ctx)
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
                        var schema = ctx.CreateDatabaseSchemaUpdater();
                        if (schema.DatabaseSchemaVersion != CURRENT_SCHEMA_VERSION)
                        {
                            ApplyMigrations(schema, targetVersion);
                            schema.Execute();
                        }
                    }
                }
            });
        }

        private static void ApplyMigrations(DatabaseSchemaUpdater schema, Version targetVersion)
        {
            // Schema 0 is the default
            // This could be any Version from 0 up to 0.9.9 inclusive
            if (schema.DatabaseSchemaVersion == 0)
            {
                if (targetVersion >= new Version(0, 9, 8))
                {
                    AddMultimediaTimeStamp(schema);
                }
                if (targetVersion >= new Version(0, 9, 9, 1))
                {
                    schema.DatabaseSchemaVersion = 1;
                }
            }
        }

        /// <summary>
        /// This Method adds the TimeStamp Column for MMOs, if it doesn't exist
        /// Change made in Commit 22a87e3bfd878951116798ae68ff81b5dd6fa26d
        /// Version 0.9.8
        /// </summary>
        /// <param name="schema"></param>
        private static void AddMultimediaTimeStamp(DatabaseSchemaUpdater schema)
        {
            try
            {
                var q = from mmo in schema.Context.GetTable<MultimediaObject>()
                        select mmo.TimeStamp;

                if (q.Any(ts => ts.HasValue))
                {
                    return;
                }
            }
            catch (Exception)
            {
                schema.AddColumn<MultimediaObject>("TimeStamp");
            }
        }
    }
}