﻿using System.Data.Linq;
using Svc = DiversityPhone.DiversityService;
using System.Collections.Generic;
using DiversityPhone.Model;
using System;
using System.Linq;
namespace DiversityPhone.Services
{
    public class TaxonService : ITaxonService
    {
        #region TaxonNames

        public void addTaxonNames(IEnumerable<TaxonName> taxa, Svc.TaxonList list)
        {
            int tableIdx = -1;
            lock (this)
            {
                withSelections(ctx =>
                {
                    var existingSelection = (from ts in ctx.TaxonSelection
                                             where ts.TableName == list.Table
                                             select ts).FirstOrDefault();
                    if (existingSelection != null)
                    {
                        tableIdx = existingSelection.TableID;
                    }
                    else
                    {
                        var unusedIDs = getUnusedTaxonTableIDs(ctx);
                        if (unusedIDs.Count() > 0)
                        {
                            var currentlyselectedTable = getTaxonTableIDForGroup(list.TaxonomicGroup);
                            var selection = new TaxonSelection()
                            {
                                TableDisplayName = list.DisplayText,
                                TableID = unusedIDs.First(),
                                TableName = list.Table,
                                TaxonomicGroup = list.TaxonomicGroup,
                                IsSelected = !TaxonSelection.ValidTableIDs.Contains(currentlyselectedTable) //If this is the first table for this group, select it.
                            };
                            ctx.TaxonSelection.InsertOnSubmit(selection);
                            ctx.SubmitChanges();
                            tableIdx = selection.TableID;
                        }
                        else
                            throw new InvalidOperationException("No Unused Taxon Table");
                    }
                });
            }

            using (var taxctx = new TaxonDataContext(tableIdx))
            {
                taxctx.TaxonNames.InsertAllOnSubmit(taxa);
                try
                {
                    taxctx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debugger.Break();
                    //TODO Log
                }
            }
        }

        public IList<TaxonSelection> getTaxonSelections()
        {
            IList<TaxonSelection> result = null;
            withSelections(ctx =>
            {
                result = (ctx.TaxonSelection.ToList());
            });
            return result;
        }

        public void selectTaxonList(Svc.TaxonList list)
        {
            withSelections(ctx =>
            {
                var tables = from s in ctx.TaxonSelection
                             where s.TaxonomicGroup == list.TaxonomicGroup
                             select s;
                var oldSelection = tables.FirstOrDefault(s => s.IsSelected);
                var newSelection = tables.FirstOrDefault(s => s.TableName == list.Table);
                if (newSelection != null)
                {
                    newSelection.IsSelected = true;

                    if (oldSelection != null)
                        oldSelection.IsSelected = false;

                    ctx.SubmitChanges();
                }
                else
                {
                    //TODO Log
                }
            }
            );
        }

        public void deleteTaxonList(Svc.TaxonList list)
        {
            withSelections(ctx =>
            {
                var selection = (from sel in ctx.TaxonSelection
                                 where sel.TableName == list.Table && sel.TaxonomicGroup == list.TaxonomicGroup
                                 select sel).FirstOrDefault();

                if (selection != null)
                {
                    using (var taxa = new TaxonDataContext(selection.TableID))
                    {
                        taxa.DeleteDatabase();
                    }
                    ctx.TaxonSelection.DeleteOnSubmit(selection);
                    ctx.SubmitChanges();
                }
                else
                {
                    //TODO Log
                }
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
                || (tableID = getTaxonTableIDForGroup(taxonGroup.Code)) == -1)
            {
                System.Diagnostics.Debugger.Break();
                //TODO Logging
                return new List<TaxonName>();
            }

            return getTaxonNames(tableID, query);
        }

        private IEnumerable<int> getUnusedTaxonTableIDs(TaxonSelectionDataContext ctx)
        {
            var usedTableIDs = from ts in ctx.TaxonSelection
                               select ts.TableID;
            return TaxonSelection.ValidTableIDs.Except(usedTableIDs);
        }

        private IList<TaxonName> getTaxonNames(int tableID, string query)
        {
            var queryWords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var q = from tn in (new TaxonDataContext(tableID).TaxonNames)
                    select tn;
            foreach (var word in queryWords)
            {
                q = q.Where(tn => tn.TaxonNameCache.Contains(word));
            }

            q = q.Take(10);

            return q.ToList();
        }

        private int getTaxonTableIDForGroup(string taxonGroup)
        {
            int id = -1;
            if (taxonGroup != null)
                withSelections(ctx =>
                {
                    var assignment = from a in ctx.TaxonSelection
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
            public Table<TaxonSelection> TaxonSelection;
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
            public Table<TaxonName> TaxonNames;
        }
    }
}