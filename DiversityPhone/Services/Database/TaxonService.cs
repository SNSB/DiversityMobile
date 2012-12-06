using System.Data.Linq;
using Svc = DiversityPhone.DiversityService;
using System.Collections.Generic;
using DiversityPhone.Model;
using System;
using System.Linq;
using System.Data.Linq.SqlClient;
namespace DiversityPhone.Services
{
    public class TaxonService : ITaxonService
    {
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
                catch (Exception e)
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

        public void deleteTaxonList(TaxonList list)
        {
            if (list == null)
                return;
            if (!TaxonList.ValidTableIDs.Contains(list.TableID))
                throw new ArgumentException("list");

            withSelections(ctx =>
            {
                using (var taxa = new TaxonDataContext(list.TableID))
                {
                    taxa.DeleteDatabase();
                }
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

        public IList<TaxonName> getTaxonNames(Term taxonGroup, string query)
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

        private IList<TaxonName> getTaxonNames(int tableID, string query)
        {
            
            var queryWords = from word in query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                             select word;

            var allTaxa = from tn in (new TaxonDataContext(tableID).TaxonNames)                    
                    select tn;

            if (queryWords.Any())
            {

                var genus = from tn in allTaxa
                            where tn.GenusOrSupragenic.StartsWith(queryWords.First())
                            select tn;


                if (queryWords.First().Equals("%"))
                {
                    genus = from tn in allTaxa
                            //where SqlMethods.Like(tn.GenusOrSupragenic, queryWords.First())
                            select tn;

                }

                if (queryWords.Count() >= 2)//Search for Genus and Epithet
                {
                    
                    var species = from tn in genus
                                  where tn.SpeciesEpithet.StartsWith(queryWords.Skip(1).First())
                                  select tn;
                    if (queryWords.Skip(1).First().Equals("%"))
                    {
                        species = from tn in genus
                                  //where SqlMethods.Like(tn.SpeciesEpithet, queryWords.Skip(1).First())
                                  select tn;
                    }

                    if (queryWords.Count() >= 3)
                    {
                      
                        var infra = from tn in species
                                    where tn.InfraspecificEpithet.StartsWith(queryWords.Skip(2).First())
                                    select tn;//Initialization
                        if (queryWords.Skip(2).First().Equals("%"))
                        {
                            infra = from tn in species
                                    //where SqlMethods.Like(tn.InfraspecificEpithet, queryWords.Skip(2).First())
                                    select tn;
                        }
     
                        if (queryWords.Count() > 3)
                        {
                            var completeQ = from inf in infra.AsEnumerable()
                                        where queryWords.Skip(3).All(word => inf.TaxonNameCache.Contains(word))
                                        orderby inf.GenusOrSupragenic, inf.SpeciesEpithet, inf.InfraspecificEpithet
                                        select inf;

                            if (completeQ.Count() > 0)
                                return completeQ.Take(20).ToList();
                            else
                                return new List<TaxonName>();
                        }
                        else
                        {
                            var completeQ = from inf in infra.AsEnumerable()
                                        orderby inf.GenusOrSupragenic, inf.SpeciesEpithet, inf.InfraspecificEpithet
                                        select inf;

                            if (completeQ.Count() > 0)
                                return completeQ.Take(20).ToList();
                            else
                                return new List<TaxonName>();
                        }
                    }
                    else
                    {
                        var completeQ = from spec in species.AsEnumerable()
                                    orderby spec.GenusOrSupragenic, spec.SpeciesEpithet, spec.InfraspecificEpithet
                                    select spec;

                        if (completeQ.Count() > 0)
                            return completeQ.Take(20).ToList();
                        else
                            return new List<TaxonName>();
                    }
                }
                else
                {
                    var completeQ = from gen in genus.AsEnumerable()
                                orderby gen.GenusOrSupragenic, gen.SpeciesEpithet, gen.InfraspecificEpithet
                                select gen;

                    if (completeQ.Count() > 0)
                        return completeQ.Take(20).ToList();
                    else
                        return new List<TaxonName>();
                }

            }
            else
                return allTaxa.Take(20).ToList();         
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
            private static string connStr = "isostore:/taxonDB{0}.sdf";

            public TaxonDataContext(int idx)
                : base(String.Format(connStr, idx))
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
