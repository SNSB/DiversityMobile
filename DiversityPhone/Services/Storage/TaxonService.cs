using DiversityPhone.Interface;
using DiversityPhone.Model;
using Microsoft.Phone.Data.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiversityPhone.Services
{
    public class TaxonService : ITaxonService, IEnableLogger
    {
        private const string WILDCARD = "%";

        #region TaxonNames

        public void addTaxonList(TaxonList list)
        {
            if (TaxonList.ValidTableIDs.Contains(list.TableID))
                throw new ArgumentException("list");

            lock (this)
            {
                withSelections(ctx =>
                {
                    var unusedIDs = getUnusedTaxonTableIDs(ctx);
                    if (unusedIDs.Count() > 0)
                    {
                        list.IsSelected = true;
                        list.TableID = unusedIDs.First();
                        ctx.TaxonLists.InsertOnSubmit(list);
                        ctx.SubmitChanges();
                    }
                    else
                        throw new InvalidOperationException("No Unused Taxon Table");
                });
            }
        }

        public void addTaxonNames(IEnumerable<TaxonName> taxa, TaxonList list)
        {
            if (!TaxonList.ValidTableIDs.Contains(list.TableID))
                throw new ArgumentException("list");

            using (var taxctx = new TaxonDataContext(list.TableID))
            {
                taxctx.TaxonNames.InsertAllOnSubmit(taxa);
                try
                {
                    taxctx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    this.Log().ErrorException("Adding Taxon Names", ex);
                }
            }
        }

        public IEnumerable<TaxonList> getTaxonLists()
        {
            using (var ctx = new TaxonSelectionDataContext())
            {
                foreach (var list in ctx.TaxonLists)
                    yield return list;
            }
        }

        public void updateTaxonList(TaxonList list)
        {
            if (list == null || !TaxonList.ValidTableIDs.Contains(list.TableID))
            {
                Debugger.Break();
                return;
            }

            withSelections(ctx =>
            {
                try
                {
                    var original = (from l in ctx.TaxonLists
                                    where l.TableID == list.TableID
                                    select l).FirstOrDefault();

                    if (original == null)
                    {
                        this.Log().Error("Tried to update nonexistent list");
                        return;
                    }

                    // The only Properties that can be updated are the following
                    original.IsSelected = list.IsSelected;

                    ctx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    this.Log().ErrorException("updateTaxonList", ex);
                }
            }
            );
        }

        public void deleteTaxonListIfExists(TaxonList list)
        {
            if (list == null || !TaxonList.ValidTableIDs.Contains(list.TableID))
                return;

            using (var isostore = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                var dbFile = TaxonDataContext.getDBPath(list.TableID);
                if (isostore.FileExists(dbFile))
                {
                    isostore.DeleteFile(dbFile);
                }
            }

            withSelections(ctx =>
            {
                ctx.TaxonLists.Attach(list);
                ctx.TaxonLists.DeleteOnSubmit(list);
                ctx.SubmitChanges();
                list.TableID = TaxonList.InvalidTableID;
            });
        }

        public int getTaxonTableFreeCount()
        {
            int result = 0;
            withSelections(ctx =>
            {
                result = getUnusedTaxonTableIDs(ctx).Count();
            });
            return result;
        }

        public IEnumerable<TaxonName> getTaxonNames(Term taxonGroup, string query)
        {
            IEnumerable<TaxonList> tables;
            if (taxonGroup == null ||
                string.IsNullOrWhiteSpace(query) ||
                !(tables = getSelectedTaxonTablesForGroup(taxonGroup.Code)).Any())
            {
                return new List<TaxonName>();
            }

            return getTaxonNames(tables, query);
        }

        public void clearTaxonLists()
        {
            withSelections(sel =>
            {
                foreach (var list in sel.TaxonLists)
                {
                    withTaxonTable(list.TableID, taxa => taxa.DeleteDatabase());
                }

                sel.DeleteDatabase();
            });
        }

        private IEnumerable<int> getUnusedTaxonTableIDs(TaxonSelectionDataContext ctx)
        {
            var usedTableIDs = from ts in ctx.TaxonLists
                               select ts.TableID;
            return TaxonList.ValidTableIDs.Except(usedTableIDs);
        }

        private IEnumerable<TaxonName> getTaxonNames(IEnumerable<TaxonList> tablesToSearch, string query)
        {
            var queryWords = (from word in query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                              select word).ToArray();

            foreach (var table in tablesToSearch)
            {
                using (var ctx = new TaxonDataContext(table.TableID))
                {
                    var q = ctx.TaxonNames as IQueryable<TaxonName>;

                    //Match Genus
                    if (queryWords.Length > 0 && queryWords[0] != WILDCARD)
                    {
                        q = from tn in q
                            where tn.GenusOrSupragenic.StartsWith(queryWords[0])
                            select tn;
                    }

                    //Match SpeciesEpithet
                    if (queryWords.Length > 1 && queryWords[1] != WILDCARD)
                    {
                        q = from tn in q
                            where tn.SpeciesEpithet.StartsWith(queryWords[1])
                            select tn;
                    }

                    //Match Infra
                    if (queryWords.Length > 2 && queryWords[2] != WILDCARD)
                    {
                        q = from tn in q
                            where tn.InfraspecificEpithet.StartsWith(queryWords[2])
                            select tn;
                    }

                    //Order
                    q = from inf in q
                        orderby inf.GenusOrSupragenic, inf.SpeciesEpithet, inf.InfraspecificEpithet
                        select inf;

                    // Treating the query as an enumerable prevents the following operators
                    // from being applied on the DB
                    // which is necessary, because "Contains" is not supported in SQL CE
                    // instead, it is evaluated in application code
                    var e = q.AsEnumerable();

                    if (queryWords.Length > 3)
                    {
                        e = from inf in e
                            where queryWords.Skip(3).Where(w => w != WILDCARD).All(word => inf.TaxonNameCache.Contains(word))
                            select inf;
                    }

                    foreach (var item in e)
                    {
                        yield return item;
                    }
                }
            }
        }

        private IEnumerable<TaxonList> getSelectedTaxonTablesForGroup(string taxonGroup)
        {
            if (string.IsNullOrWhiteSpace(taxonGroup))
            {
                return Enumerable.Empty<TaxonList>();
            }

            IEnumerable<TaxonList> result = Enumerable.Empty<TaxonList>();

            withSelections(ctx =>
            {
                var assignment = from a in ctx.TaxonLists
                                 where a.TaxonomicGroup == taxonGroup && a.IsSelected
                                 select a;
                result = assignment.ToList();
            });

            return result;
        }

        private void withSelections(Action<TaxonSelectionDataContext> operation)
        {
            using (var ctx = new TaxonSelectionDataContext())
            {
                operation(ctx);
            }
        }

        private void withTaxonTable(int id, Action<TaxonDataContext> operation)
        {
            using (var ctx = new TaxonDataContext(id))
            {
                operation(ctx);
            }
        }

        #endregion TaxonNames

        private const int CURRENT_TAXA_SCHEMA_VERSION = 1; // As of Version 1.0.1

        public Task UpgradeDatabase()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    withSelections(ctx =>
                    {
                        var schema = ctx.CreateDatabaseSchemaUpdater();

                        // Version 1.0.1
                        if (schema.DatabaseSchemaVersion < 1)
                        {
                            schema.AddColumn<TaxonList>("ListID");
                            schema.DatabaseSchemaVersion = 1;
                        }

                        schema.Execute();
                    });
                }
                catch (Exception ex)
                {
                    this.Log().ErrorException("UpgradeTaxonDatabase", ex);
                }
            });
        }

        private class TaxonSelectionDataContext : DataContext
        {
            private static string connStr = "isostore:/taxonDB.sdf";
            private static object init_lock = new object();

            public TaxonSelectionDataContext()
                : base(connStr)
            {
                if (!this.DatabaseExists())
                {
                    Monitor.Enter(init_lock); // Not created, let 1 thread create it
                    if (!this.DatabaseExists())
                    {
                        this.CreateDatabase();
                    }
                    Monitor.Exit(init_lock);
                }
            }

#pragma warning disable 0649
            public Table<TaxonList> TaxonLists;
#pragma warning restore 0649
        }

        private class TaxonDataContext : DataContext
        {
            private static readonly string ISOSTORE_PROTOCOL = "isostore:/";
            private static readonly string TAXON_DB_NAME_PATTERN = "taxonDB{0}.sdf";
            private static object init_lock = new object();

            public static string getDBPath(int idx)
            {
                return string.Format(TAXON_DB_NAME_PATTERN, idx);
            }

            public TaxonDataContext(int idx)
                : base(string.Format("{0}{1}", ISOSTORE_PROTOCOL, getDBPath(idx)))
            {
                if (!this.DatabaseExists())
                {
                    Monitor.Enter(init_lock); // Not created, let 1 thread create it
                    if (!this.DatabaseExists())
                    {
                        this.CreateDatabase();
                    }
                    Monitor.Exit(init_lock);
                }
            }

#pragma warning disable 0649
            public Table<TaxonName> TaxonNames;
#pragma warning restore 0649
        }
    }
}