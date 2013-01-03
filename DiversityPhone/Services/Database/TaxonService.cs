using System.Data.Linq;
using Svc = DiversityPhone.DiversityService;
using System.Collections.Generic;
using DiversityPhone.Model;
using System;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.IO.IsolatedStorage;

namespace DiversityPhone.Services
{
    public interface ITaxonService
    {
        /// <summary>
        /// Adds a new List to the database. 
        /// After adding it, the object will have been updated with the table id.
        /// </summary>
        /// <param name="newList"></param>
        void addTaxonList(TaxonList newList);
        /// <summary>
        /// Adds new TaxonNames to the database
        /// </summary>        
        /// <param name="taxa">List of TaxonNames</param>
        /// <param name="source">Source</param>
        void addTaxonNames(IEnumerable<TaxonName> taxa, TaxonList source);
        /// <summary>
        /// Queries how many Taxon tables are unused
        /// </summary>
        /// <returns>Number of free Taxon tables</returns>
        int getTaxonTableFreeCount();
        /// <summary>
        /// Updates a Taxon Selection
        /// </summary>
        /// <param name="sel"></param>
        void selectTaxonList(TaxonList list);
        /// <summary>
        /// Gets all defined Taxon Selections
        /// </summary>
        /// <returns></returns>
        IEnumerable<TaxonList> getTaxonSelections();
        /// <summary>
        /// Removes a Taxon Selection and empties its Taxon Table
        /// </summary>
        /// <param name="list"></param>
        void deleteTaxonListIfExists(TaxonList list);

        /// <summary>
        /// Retrieves Taxon Names that conform to the query string
        /// </summary>
        /// <param name="taxGroup"></param>
        /// <param name="query">space separated list of keywords</param>
        /// <returns></returns>
        IEnumerable<TaxonName> getTaxonNames(Term taxGroup, string query);

        /// <summary>
        /// Deletes all Taxon list selections and Databases.
        /// </summary>
        void clearTaxonLists();
    }

    public class TaxonService : ITaxonService
    {

        private const string WILDCARD = "%";
        #region TaxonNames

        public void addTaxonList(TaxonList list)
        {
            if (TaxonList.ValidTableIDs.Contains(list.TableID))
                throw new ArgumentException("newList");
            
            lock (this)
            {
                withSelections(ctx =>
                {                    
                    var unusedIDs = getUnusedTaxonTableIDs(ctx);
                    if (unusedIDs.Count() > 0)
                    {
                        var currentlyselectedTable = getTaxonTableIDForGroup(list.TaxonomicGroup);
                        list.IsSelected = !TaxonList.ValidTableIDs.Contains(currentlyselectedTable); //If this is the first table for this group, select it.
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
                catch (Exception)
                {
                    System.Diagnostics.Debugger.Break();
                    //TODO Log
                }
            }
        }

        public IEnumerable<TaxonList> getTaxonSelections()
        {
            using (var ctx = new TaxonSelectionDataContext())
            {
                foreach (var list in ctx.TaxonLists)
                    yield return list;
            }
        }

        public void selectTaxonList(TaxonList list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (!TaxonList.ValidTableIDs.Contains(list.TableID))
                throw new ArgumentException("list");
            if (list.IsSelected)
                return;


            withSelections(ctx =>
            {
                var tables = from s in ctx.TaxonLists
                             where s.TaxonomicGroup == list.TaxonomicGroup
                             select s;
                var oldSelection = tables.FirstOrDefault(s => s.IsSelected);                
                if (oldSelection != null)
                {
                    oldSelection.IsSelected = false;
                }

                list.IsSelected = false;
                ctx.TaxonLists.Attach(list);
                list.IsSelected = true;
                ctx.SubmitChanges();
                               
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
            int tableID;
            if (taxonGroup == null
                || (tableID = getTaxonTableIDForGroup(taxonGroup.Code)) == TaxonList.InvalidTableID)
            {
                //System.Diagnostics.Debugger.Break();
                //TODO Logging
                return new List<TaxonName>();
            }

            return getTaxonNames(tableID, query);
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

        private IEnumerable<TaxonName> getTaxonNames(int tableID, string query)
        {
            
            var queryWords = (from word in query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                             select word).ToArray();

            using (var ctx = new TaxonDataContext(tableID))
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
            
                if (queryWords.Length > 3)
                {
                    q = from inf in q
                        where queryWords.Skip(3).Where(w => w != WILDCARD).All(word => inf.TaxonNameCache.Contains(word))                        
                        select inf;
                }

                foreach (var item in q)
                {
                    yield return item;
                }
            }
        }

        private int getTaxonTableIDForGroup(string taxonGroup)
        {
            int id = TaxonList.InvalidTableID;
            if (taxonGroup != null)
                withSelections(ctx =>
                {
                    var assignment = from a in ctx.TaxonLists
                                     where a.TaxonomicGroup == taxonGroup && a.IsSelected
                                     select a.TableID;
                    if (assignment.Any())
                        id = assignment.First();
                });
            return id;
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
        
        #endregion

        private class TaxonSelectionDataContext : DataContext
        {
            private static string connStr = "isostore:/taxonDB.sdf";

            public TaxonSelectionDataContext()
                : base(connStr)
            {
                if (!this.DatabaseExists())
                    this.CreateDatabase();
            }
#pragma warning disable 0649
            public Table<TaxonList> TaxonLists;
#pragma warning restore 0649
        }

        private class TaxonDataContext : DataContext
        {
            private static readonly string ISOSTORE_PROTOCOL = "isostore:/";
            private static readonly string TAXON_DB_NAME_PATTERN = "taxonDB{0}.sdf";

            public static string getDBPath(int idx)
            {
                return String.Format(TAXON_DB_NAME_PATTERN, idx);
            }


            public TaxonDataContext(int idx)
                : base(String.Format("{0}{1}",ISOSTORE_PROTOCOL,getDBPath(idx)))
            {
                if (!this.DatabaseExists())
                    this.CreateDatabase();
            }
#pragma warning disable 0649
            public Table<TaxonName> TaxonNames;
#pragma warning restore 0649
        }




        
    }
}
