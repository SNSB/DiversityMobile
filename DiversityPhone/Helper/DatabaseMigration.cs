namespace DiversityPhone.Helper
{
    using DiversityPhone.Model;
    using DiversityPhone.Services;
    using Microsoft.Phone.Data.Linq;
    using ReactiveUI;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class VersionMigration
    {
        public const int CURRENT_FIELD_DATA_SCHEMA_VERSION = 2; // As of Version 0.9.10.0/1.0

        public static Task CreateOrUpgradeFieldData(string dbPath = null, Version targetVersion = null)
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
                        schema.DatabaseSchemaVersion = CURRENT_FIELD_DATA_SCHEMA_VERSION;
                        schema.Execute();
                    }
                    else
                    {
                        var schema = ctx.CreateDatabaseSchemaUpdater();
                        if (schema.DatabaseSchemaVersion != CURRENT_FIELD_DATA_SCHEMA_VERSION)
                        {
                            ApplyMigrations(schema, targetVersion ?? GetCurrentVersionNumber());
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
                schema.DatabaseSchemaVersion = 1;
            }
            if (schema.DatabaseSchemaVersion == 1)
            {
                if (targetVersion >= new Version(0, 9, 9, 1))
                {
                    AddLocalization(schema);
                }
                schema.DatabaseSchemaVersion = 2;
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

        /// <summary>
        /// This Method adds the Localization Table, if it doesn't exist
        /// Version 0.9.10.0/1.0
        /// </summary>
        /// <param name="schema"></param>
        private static void AddLocalization(DatabaseSchemaUpdater schema)
        {
            var ctx = schema.Context;

            try
            {
                var locTable = ctx.GetTable<Localization>();
                if (locTable.Any())
                {
                    LogManager.GetLogger(typeof(VersionMigration)).Info("Localization Table already exists");
                }
            }
            catch (Exception ex)
            {
                schema.AddTable<Localization>();
            }
        }
    }
}