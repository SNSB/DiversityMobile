namespace DiversityPhone.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using ReactiveUI;
    using DiversityPhone.Messages;
    using DiversityPhone.Common;
    using System.Data.Linq;
    using System.Linq.Expressions;
    using Svc = DiversityPhone.DiversityService;

    public class OfflineStorage : IOfflineStorage
    {
        private IList<IDisposable> _subscriptions;
        private IMessageBus _messenger;

        public OfflineStorage(IMessageBus messenger)
        {
            this._messenger = messenger;

            _subscriptions = new List<IDisposable>()
            {
                

                _messenger.Listen<EventSeries>(MessageContracts.SAVE)
                    .Subscribe(es => addOrUpdateEventSeries(es)),
                _messenger.Listen<EventSeries>(MessageContracts.DELETE)
                    .Subscribe(es => deleteEventSeries(es)),
                _messenger.Listen<Event>(MessageContracts.SAVE)
                    .Subscribe(ev => addOrUpdateEvent(ev)),

                _messenger.Listen<Specimen>(MessageContracts.SAVE)
                    .Subscribe(spec => addOrUpdateSpecimen(spec)),

                _messenger.Listen<IdentificationUnit>(MessageContracts.SAVE)
                    .Subscribe(iu => addOrUpdateIUnit(iu)),
                _messenger.Listen<MultimediaObject>(MessageContracts.SAVE)
                    .Subscribe(mmo => addMultimediaObject(mmo)),
            };

            using (var context = new DiversityDataContext())
            {
                if (!context.DatabaseExists())
                    context.CreateDatabase();
            }
        }

        #region UserProfile

        public IList<UserProfile> getAllUserProfiles()
        {
            return uncachedQuery(ctx => ctx.Profiles);
        }
        public UserProfile getUserProfile(string loginName)
        {
            return singletonQuery(ctx => ctx.Profiles
                .Where(p => p.LoginName == loginName));
        }

        #endregion

        #region EventSeries

        private IList<EventSeries> esQuery(Expression<Func<EventSeries, bool>> restriction = null)
        {
            return cachedQuery(EventSeries.Operations,
            ctx =>
            {
                var q = (from es in ctx.EventSeries
                         select es);
                if (restriction == null)
                    return q;
                else
                    return q.Where(restriction);
            });
        }

        public IList<EventSeries> getAllEventSeries()
        {
            return esQuery();
        }

        public IList<EventSeries> getNewEventSeries()
        {
            return esQuery(es => es.ModificationState == null);
        }

        public EventSeries getEventSeriesByID(int id)
        {            
            return singletonQuery(ctx => from es in ctx.EventSeries
                                         where es.SeriesID == id
                                         select es);

        }

        public void addOrUpdateEventSeries(global::DiversityPhone.Model.EventSeries newSeries)
        {
            if (EventSeries.isNoEventSeries(newSeries))
                return;

            addOrUpdateRow(EventSeries.Operations, ctx => ctx.EventSeries, newSeries);
            EventSeries.Actual = newSeries;
        }

        public void deleteEventSeries(EventSeries toDeleteEs)
        {
            deleteRow(EventSeries.Operations, ctx => ctx.EventSeries, toDeleteEs);
        }

        #endregion

        #region Event

        public IList<Event> getAllEvents()
        {
            return cachedQuery(Event.Operations,
            ctx =>
                from ev in ctx.Events                
                select ev
                );

        }


        public IList<Event> getEventsForSeries(EventSeries es)
        {
            if (EventSeries.isNoEventSeries(es))
                return getEventsWithoutSeries();

            return cachedQuery(Event.Operations,
            ctx =>
                from ev in ctx.Events
                where ev.SeriesID == es.SeriesID 
                select ev
                );
        }

        private IList<Event> getEventsWithoutSeries()
        {
            return cachedQuery(Event.Operations,
            ctx =>
                from ev in ctx.Events
                where ev.SeriesID == null
                select ev
                );
        }     

        public Event getEventByID(int id)
        {
            return singletonQuery(
                ctx => from ev in ctx.Events
                       where ev.EventID == id
                       select ev);
        }
          

        public void addOrUpdateEvent(Event ev)
        {
            var wasNewEvent = ev.IsNew();

            addOrUpdateRow(Event.Operations,
                  ctx => ctx.Events,
                  ev
              );

            if (wasNewEvent)
            {
                //Now EventID is set even for new Events
                Specimen observation = new Specimen().MakeObservation();
                observation.CollectionEventID = ev.EventID;
                addOrUpdateSpecimen(observation);
            }
        }
        #endregion

        #region CollectionEventProperties

        public IList<CollectionEventProperty> getPropertiesForEvent(Event ev)
        {
            return cachedQuery(CollectionEventProperty.Operations,
            ctx =>
                from cep in ctx.CollectionEventProperties
                where cep.EventID == ev.EventID 
                select cep
                );
        }

        public void addOrUpdateCollectionEventProperty(CollectionEventProperty cep)
        {
            addOrUpdateRow(CollectionEventProperty.Operations,
                  ctx => ctx.CollectionEventProperties,
                  cep
              );
        }

        public IList<Property> getAllProperties()
        {
            return uncachedQuery(ctx => from p in ctx.Properties
                                        select p);
        }

        public Property getPropertyByID(int id)
        {
            return singletonQuery(ctx =>
                from p in ctx.Properties
                where p.PropertyID == id
                select p);
        }
        #endregion


        #region Specimen        
        
        public IList<Specimen> getAllSpecimen()
        {
            
            return cachedQuery(Specimen.Operations,
            ctx =>            
				from spec in ctx.Specimen
                select spec
            );
        }
        

        public IList<Specimen> getSpecimenForEvent(Event ev)
        {
            
            return cachedQuery(Specimen.Operations,
            ctx =>
                from spec in ctx.Specimen                 
                where spec.CollectionEventID == ev.EventID
                select spec
                );
        
        }
      

        public Specimen getSpecimenByID(int id)
        {
            return singletonQuery(
                ctx => from spec in ctx.Specimen
                       where spec.CollectionSpecimenID == id
                       select spec);
        }

        public IList<Specimen> getSpecimenWithoutEvent()
        {
          return cachedQuery(Specimen.Operations,
            ctx =>
                from spec in ctx.Specimen
                where spec.CollectionEventID == null 
                select spec
                );
        }

        public void addOrUpdateSpecimen(Specimen spec)
        {
            addOrUpdateRow(Specimen.Operations,
                ctx => ctx.Specimen,
                spec
            );
        }

        #endregion

        #region IdentificationUnit

        public IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec)
        {
            return cachedQuery(IdentificationUnit.Operations,
            ctx =>
                from iu in ctx.IdentificationUnits
                where iu.SpecimenID == spec.CollectionSpecimenID && iu.RelatedUnitID == null 
                select iu
                );
        }


        public IList<IdentificationUnit> getSubUnits(IdentificationUnit unit)
        {
            return cachedQuery(IdentificationUnit.Operations,
            ctx =>
                from iu in ctx.IdentificationUnits
                where iu.RelatedUnitID == unit.UnitID 
                select iu
                );
        }

        public IdentificationUnit getIdentificationUnitByID(int id)
        {
            IdentificationUnit result = null;
            withDataContext((ctx) =>
                {
                    result = (from iu in ctx.IdentificationUnits
                              where iu.UnitID == id
                              select iu).FirstOrDefault();
                });
            return result;
        }

        private static int findFreeUnitID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.IdentificationUnits.Any())
                min = (from iu in ctx.IdentificationUnits select iu.UnitID).Min();
            return (min > -1) ? -1 : min - 1;
        }

        public void addOrUpdateIUnit(IdentificationUnit iu)
        {
            using (var ctx = new DiversityDataContext())
            {
                if (iu.ModificationState == null)
                    iu.UnitID = findFreeUnitID(ctx);
                ctx.IdentificationUnits.InsertOnSubmit(iu);
                ctx.SubmitChanges();
            }
        }

        #endregion

        #region Analyses 


        public IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu)
        {
            return cachedQuery(IdentificationUnitAnalysis.Operations,
            ctx =>
                from iuan in ctx.IdentificationUnitAnalyses
                where iuan.IdentificationUnitID == iu.UnitID 
                select iuan
                );
        }


        public IdentificationUnitAnalysis getIUANByID(int iuanalysisID)
        {
            return singletonQuery(ctx => from iuan in ctx.IdentificationUnitAnalyses
                                         where iuan.IdentificationUnitAnalysisID == iuanalysisID
                                         select iuan);
        }

        public void addOrUpdateIUA(IdentificationUnitAnalysis iua)
        {
            addOrUpdateRow(IdentificationUnitAnalysis.Operations,
                ctx => ctx.IdentificationUnitAnalyses,
                iua
            );
        }      

        public IList<Analysis> getAllAnalyses()
        {
            return cachedQuery(Analysis.Operations,
            ctx =>
                from an in ctx.Analyses                
                select an
                );
        }

        public IList<Analysis> getPossibleAnalyses(string taxonomicGroup)
        {
            return cachedQuery(Analysis.Operations,
                ctx =>
                from an in ctx.Analyses
                join atg in ctx.AnalysisTaxonomicGroups on an.AnalysisID equals atg.AnalysisID
                where atg.TaxonomicGroup == taxonomicGroup
                select an);
        }

        public Analysis getAnalysisByID(int id)
        {
            return singletonQuery(ctx => from an in ctx.Analyses
                                         where an.AnalysisID == id
                                         select an);
        }

        public void addAnalyses(IEnumerable<Analysis> analyses)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.Analyses.InsertAllOnSubmit(analyses);
                ctx.SubmitChanges();
            }
        }

        public IList<AnalysisResult> getPossibleAnalysisResults(int analysisID)
        {
            return cachedQuery(AnalysisResult.Operations,
            ctx =>
                from ar in ctx.AnalysisResults
                where ar.AnalysisID == analysisID 
                select ar
            );
        }
        public void addAnalysisResults(IEnumerable<AnalysisResult> results)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.AnalysisResults.InsertAllOnSubmit(results);
                ctx.SubmitChanges();
            }
        }


        public void addAnalysisTaxonomicGroups(IEnumerable<AnalysisTaxonomicGroup> groups)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.AnalysisTaxonomicGroups.InsertAllOnSubmit(groups);
                ctx.SubmitChanges();
            }
        }



        #endregion

        #region Multimedia

        public IList<MultimediaObject> getAllMultimediaObjects()
        {
            return uncachedQuery(ctx => ctx.MultimediaObjects);
        }

        public IList<MultimediaObject> getMultimediaForObject(DiversityPhone.Services.ReferrerType refType, int key)
        {
             IList<MultimediaObject> objects= uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.OwnerType == refType
                                                && mm.RelatedId == key
                                        select mm);
             return objects;

        }

        public MultimediaObject getMultimediaByURI(string uri)
        {
            IList<MultimediaObject> objects = uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                                                   where mm.Uri==uri
                                                                   select mm);
            if (objects.Count == 0)
                throw new KeyNotFoundException();
            if (objects.Count > 1)
                throw new DuplicateKeyException(objects);
            return objects[0];
        }
     

        public void addMultimediaObject(MultimediaObject mmo)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.MultimediaObjects.InsertOnSubmit(mmo);
                ctx.SubmitChanges();
            }
        }
        #endregion

        #region Terms

        public IList<Term> getTerms(Svc.TermList source)
        {
            return uncachedQuery(ctx => from t in ctx.Terms
                                        where t.SourceID == source
                                        select t);
        }



        public void addTerms(IEnumerable<Term> terms)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.Terms.InsertAllOnSubmit(terms);
                try
                {
                    ctx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    //TODO Log
                }
            }
            sampleData();
        }

        #endregion

        #region TaxonNames

        public void addTaxonNames(IEnumerable<TaxonName> taxa, Svc.TaxonList list)
        {
            withDataContext(ctx =>
                {
                    Table<TaxonName> targetTable = null;
                    var existingSelection = (from ts in ctx.TaxonSelection
                                        where ts.TableName == list.Table
                                        select ts).FirstOrDefault();
                    if (existingSelection != null)
                    {
                        targetTable = getTaxonTable(ctx, existingSelection.TableID);
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
                                IsSelected = TaxonSelection.ValidTableIDs.Contains(currentlyselectedTable)
                            };
                            ctx.TaxonSelection.InsertOnSubmit(selection);

                            targetTable = getTaxonTable(ctx, selection.TableID );
                        }
                        else
                            throw new InvalidOperationException("No Unused Taxon Table");
                    }

                    targetTable.InsertAllOnSubmit(taxa);
                    ctx.SubmitChanges();
                }
            );
        }

        public IList<TaxonSelection> getTaxonSelections()
        {
            IList<TaxonSelection> result = null;
            withDataContext(ctx =>
                {
                    result = (ctx.TaxonSelection.ToList());
                });
            return result;
        }

        public void updateTaxonSelection(TaxonSelection sel)
        {
            withDataContext(ctx =>
                {
                    var existingSelection = from s in ctx.TaxonSelection
                                            where s.TableID == sel.TableID
                                            select s;
                    if (existingSelection.Any())
                    {
                        var selToUdate = existingSelection.First();

                        selToUdate.IsSelected = sel.IsSelected;
                        selToUdate.TableDisplayName = sel.TableDisplayName;
                        selToUdate.TableName = sel.TableName;
                        selToUdate.TaxonomicGroup = sel.TaxonomicGroup;

                        ctx.SubmitChanges();
                    }
                    else
                    {
                        //TODO Log
                    }
                }
            );
        }

        public void clearTaxonTable(TaxonSelection selection)
        {
            withDataContext(ctx =>
                {
                    var targetTable = getTaxonTable(ctx, selection.TableID);
                    targetTable.DeleteAllOnSubmit(targetTable);

                    ctx.TaxonSelection.Attach(selection);
                    ctx.TaxonSelection.DeleteOnSubmit(selection);

                    ctx.SubmitChanges();
                });
        }

        public int getTaxonTableFreeCount()
        {
            int result = 0;
            withDataContext(ctx =>
            {
                result = getUnusedTaxonTableIDs(ctx).Count();
            });
            return result;
        }

        public IList<TaxonName> getTaxonNames(Term taxonGroup)
        {
            return getTaxonNames(taxonGroup, null, null);
        }

        public IList<TaxonName> getTaxonNames(Term taxonGroup, string genus, string species)
        {
            int tableID = getTaxonTableIDForGroup(taxonGroup.Code);
            genus = genus ?? "";
            species = species ?? "";

            if (tableID == -1)
            {
                return new List<TaxonName>();
                //TODO Logging?
            }
            else
            {
                return getTaxonNames(tableID, genus, species);
            }
        }

        private IEnumerable<int> getUnusedTaxonTableIDs(DiversityDataContext ctx)
        {
            var usedTableIDs = from ts in ctx.TaxonSelection
                               select ts.TableID;
            return TaxonSelection.ValidTableIDs.Except(usedTableIDs);           
        }

        

        private Table<TaxonName> getTaxonTable(DiversityDataContext ctx, int tableID)
        {
            switch (tableID)
            {
                case 0: return ctx.TaxonNames0;
                case 1: return ctx.TaxonNames1;
                case 2: return ctx.TaxonNames2;
                case 3: return ctx.TaxonNames3;
                case 4: return ctx.TaxonNames4;
                case 5: return ctx.TaxonNames5;
                case 6: return ctx.TaxonNames6;
                case 7: return ctx.TaxonNames7;
                case 8: return ctx.TaxonNames8;
                case 9: return ctx.TaxonNames9;
                default:
                    throw new IndexOutOfRangeException("Only 10 tables are supported. Id is not between 0 and 9");
            }
        }      

        private IList<TaxonName> getTaxonNames(int tableID, string genus, string species)
        {
            return cachedQuery(TaxonName.Operations,
                ctx => from tn in getTaxonTable(ctx, tableID)
                        where tn.GenusOrSupragenic.Contains(genus)
                        && tn.SpeciesEpithet.Contains(species)
                        select tn);
        }

        private int getTaxonTableIDForGroup(string taxonGroup)
        {
            int id = -1;
            if(taxonGroup != null)
                withDataContext(ctx =>
                {
                    var assignment = from a in ctx.TaxonSelection
                                     where a.TableName == taxonGroup && a.IsSelected
                                     select a.TableID;
                    if (assignment.Any())
                        id = assignment.First();
                });
            return id;
        }

        


        #endregion

        #region PropertyNames

        public void addPropertyNames(IEnumerable<PropertyName> properties)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.PropertyNames.InsertAllOnSubmit(properties);
                ctx.SubmitChanges();
            }
        }

        public IList<PropertyName> getPropertyNames(Property prop)
        {
            return uncachedQuery(ctx => from pn in ctx.PropertyNames
                                        where pn.PropertyID == prop.PropertyID
                                        select pn);
        }

        public PropertyName getPropertyNameByURI(string uri)
        {
            PropertyName result = null;

            withDataContext(ctx =>
                {
                    result = (from pn in ctx.PropertyNames
                              where pn.PropertyUri == uri
                              select pn).FirstOrDefault();
                });
            return result;
        }

        #endregion

        #region Maps

        public IList<Map> getAllMaps()
        {
            return uncachedQuery(ctx => from m in ctx.Maps
                                        select m);
        }
        public IList<Map> getMapsForRectangle(double latitudeNorth, double latitudeSouth, double longitudeWest, double longitudeEast)
        {
            throw new NotImplementedException();
        }

        public void addMap(Map map)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.Maps.InsertOnSubmit(map);
                ctx.SubmitChanges();
            }
        }
        #endregion


        #region SampleData
        private void sampleData()
        {
            //using (var ctx = new DiversityDataContext())
            //{
            //    ctx.EventSeries.InsertOnSubmit(new Model.EventSeries() { SeriesID = 1, Description = "ES" });
            //    ctx.Events.InsertOnSubmit(new Model.Event() { SeriesID = 0, EventID = 0, LocalityDescription = "EV" });
            //    ctx.Specimen.InsertOnSubmit(new Model.Specimen() { CollectionEventID = 0, CollectionSpecimenID = 0, AccessionNumber = "CS" });
            //    ctx.IdentificationUnits.InsertOnSubmit(new IdentificationUnit() { SpecimenID = 0, UnitID = 0 });
            //    int id = 1;
            //    recSample(0, 0, ref id, ctx);
            //    ctx.SubmitChanges();
            //}

        }
        private void recSample(int depth, int parent, ref int id, DiversityDataContext ctx)
        {
            if (depth == 3)
                return;

            depth++;
            int p = id;


            for (int i = 0; i < 20; i++)
            {
                ctx.IdentificationUnits.InsertOnSubmit(new IdentificationUnit() { UnitID = id, RelatedUnitID = parent });
                recSample(depth, id++, ref id, ctx);
            }
        }

        #endregion

        #region Generische Implementierungen
        private void addOrUpdateRow<T>(IQueryOperations<T> operations, TableProvider<T> tableProvider, T row) where T : class, IModifyable
        {
            if(row == null)
            {
#if DEBUG
                throw new ArgumentNullException ("row");
#else
                return;
#endif
            }

            withDataContext((ctx) =>
                {
                    var table = tableProvider(ctx);
                    var allRowsQuery = table as IQueryable<T>;



                    if (row.IsNew())      //New Object
                    {
                        operations.SetFreeKeyOnItem(allRowsQuery, row);
                        row.ModificationState = true; //Mark for Upload

                        table.InsertOnSubmit(row);                        
                        try
                        {
                            ctx.SubmitChanges();                            
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debugger.Break();
                            
                            //Object not new
                            //TODO update?
                        }
                    }
                    else
                    {
                        var existingRow = operations.WhereKeyEquals(allRowsQuery, row)
                                                    .FirstOrDefault();
                        if (existingRow != default(T))
                        {
                            //Second DataContext necessary 
                            //because the action of querying for an existing row prevents a new version of that row from being Attach()ed
                            withDataContext((ctx2) =>
                                {
                                    tableProvider(ctx2).Attach(row, existingRow);
                                    ctx2.SubmitChanges();
                                });
                        }
                    }              
                });
        }

        private void deleteRow<T>(IQueryOperations<T> operations, TableProvider<T> tableProvider, T detachedRow) where T : class
        {

            withDataContext(ctx =>
                {
                    var table = tableProvider(ctx);
                    var attachedRow = operations.WhereKeyEquals(table, detachedRow)
                        .FirstOrDefault();

                    if (attachedRow != null)
                    {
                        table.DeleteOnSubmit(attachedRow);
                        ctx.SubmitChanges();
                    }
                });
        }

        private T singletonQuery<T>(QueryProvider<T> queryProvider)
        {
            T result = default(T);
            withDataContext(ctx =>
                {
                    var query = queryProvider(ctx);
                    result = query
                        .FirstOrDefault();
                });
            return result;
        }


        private void withDataContext(Action<DiversityDataContext> operation)
        {
            using (var ctx = new DiversityDataContext())
                operation(ctx);
        }

        private IList<T> cachedQuery<T>(IQueryOperations<T> operations, QueryProvider<T> queryProvider) where T : class
        {
            return new RotatingCache<T>(new QueryCacheSource<T>(operations, queryProvider));
        }

        private IList<T> uncachedQuery<T>(QueryProvider<T> query)
        {
            IList<T> result = null;
            withDataContext(ctx => result = query(ctx).ToList());
            return result;
        }


        private class QueryCacheSource<T> : ICacheSource<T>
        {
            IQueryOperations<T> operations;
            QueryProvider<T> queryProvider;

            public QueryCacheSource(IQueryOperations<T> operations, QueryProvider<T> queryProvider)
            {
                this.operations = operations;
                this.queryProvider = queryProvider;
            }

            public IEnumerable<T> retrieveItems(int count, int offset)
            {
                using (var ctx = new DiversityDataContext())
                {
                    return queryProvider(ctx)
                        .Skip(offset)
                        .Take(count)
                        .ToList();
                }
            }

            public int Count
            {
                get
                {
                    using (var ctx = new DiversityDataContext())
                    {
                        return queryProvider(ctx)
                            .Count();
                    }
                }
            }


            public int IndexOf(T item)
            {
                using (var ctx = new DiversityDataContext())
                {
                    return operations.WhereKeySmallerThan(queryProvider(ctx), item)
                        .Count();
                }
            }

        }

        private delegate Table<T> TableProvider<T>(DiversityDataContext ctx) where T : class;

        private delegate IQueryable<T> QueryProvider<T>(DiversityDataContext ctx);       

        #endregion

        #region IOfflineFieldData Members

        public Svc.HierarchySection getNewHierarchyBelow(Event ev)
        {
            throw new NotImplementedException();
        }

        public void updateHierarchy(Svc.HierarchySection from, Svc.HierarchySection to)
        {
            throw new NotImplementedException();
        }

        #endregion








    }
}
