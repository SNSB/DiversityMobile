namespace DiversityPhone.Services
{
using System;
using System.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using DiversityPhone.Utility;
using DiversityPhone.Common;
using System.Data.Linq;
using System.Linq.Expressions;
using DiversityPhone.Utility;
using Svc = DiversityPhone.Service;

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
            var ctx = new DiversityDataContext();
            return new LightList<UserProfile>(ctx.Profiles);
        }
        public UserProfile getUserProfile(string loginName)
        {
            var ctx = new DiversityDataContext();
            LightList<UserProfile> profileList = new LightList<UserProfile>(from prof in ctx.Profiles
                                                                       where prof.LoginName.Equals(loginName)
                                                                       select prof);
            if (profileList.Count == 0)
                throw new KeyNotFoundException("No profile with loginname: " + loginName);
            else if (profileList.Count > 1)
                throw new Utility.PrimaryKeyViolationException("Multiple values for id: " + loginName);
            else return profileList[0];
        }

        #endregion

        #region EventSeries

        private IList<EventSeries> esQuery(Expression<Func<EventSeries, bool>> restriction = null)
        {
            return cachedSingleKeyedQuery(ctx => 
                {
                    if(restriction == null)
                        return (from es in ctx.EventSeries
                                select es);
                    else
                        return (from es in ctx.EventSeries
                                select es).Where(restriction);
                },
                es => es.SeriesID);
        }

        public IList<EventSeries> getAllEventSeries()
        {
            return esQuery();
        }

        public IList<EventSeries> getNewEventSeries()
        {
           return esQuery(es => es.IsModified == null);
        }

        public EventSeries getEventSeriesByID(int id)
        {
            EventSeries result = null;
            withDataContext((ctx) =>
                {
                    result = (from es in ctx.EventSeries
                              where es.SeriesID == id
                              select es).FirstOrDefault();
                });
            return result;
        }

        public EventSeries getEventSeriesByID(int id, DiversityDataContext ctx)
        {
            EventSeries result = null;
                result = (from es in ctx.EventSeries
                          where es.SeriesID == id
                          select es).FirstOrDefault();
            return result;
        }

        private static int findFreeEventSeriesID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.EventSeries.Any())
                min = (from es in ctx.EventSeries select es.SeriesID).Min();
            return (min > -1) ? -1 : min - 1;
        }

        public void addOrUpdateEventSeries(global::DiversityPhone.Model.EventSeries newSeries)
        {
            if (EventSeries.isNoEventSeries(newSeries))
                return;
            using (var ctx = new DiversityDataContext())
            {
                newSeries.LogUpdatedWhen = DateTime.Now;
                if (newSeries.SeriesID == 0) //Entspricht Insert
                {
                    newSeries.SeriesID = findFreeEventSeriesID(ctx);
                    ctx.EventSeries.InsertOnSubmit(newSeries);
                }
                else
                {
                    EventSeries storedSeries = this.getEventSeriesByID(newSeries.SeriesID,ctx);
                    if (storedSeries != null) //Update
                    {
                        ReflectionOperations.copyAllFields(newSeries, storedSeries);
                        var changeSet = ctx.GetChangeSet();
                        IList<Object> updates = changeSet.Updates;
                    }
                    else //Insert. Der Fall darf aber eigentlich nicht auftreten.
                        //ctx.EventSeries.InsertOnSubmit(newSeries);
                        throw new ArgumentOutOfRangeException();
                }
                ctx.SubmitChanges();
            }
        }

        public void deleteEventSeries(EventSeries toDeleteEs)
        {
            var ctx = new DiversityDataContext();
            var delete = (from es in ctx.EventSeries
                      where es.SeriesID == toDeleteEs.SeriesID
                      select es).FirstOrDefault();
            ctx.EventSeries.DeleteOnSubmit(delete);
            ctx.SubmitChanges();
        }

        #endregion

        #region Event

        private IList<Event> evQuery(Expression<Func<Event, bool>> restriction = null)
        {
            return cachedSingleKeyedQuery(ctx =>
            {
                if (restriction == null)
                    return (from ev in ctx.Events
                            select ev);
                else
                    return (from ev in ctx.Events
                            select ev).Where(restriction);
            },
                ev => ev.EventID);
        }

        public IList<Event> getAllEvents()
        {
            return evQuery();

        }


        public IList<Event> getEventsForSeries(EventSeries es)
        {
            return evQuery(ev => ev.SeriesID == es.SeriesID);
        }

        public IList<Event> getEventsWithoutSeries()
        {
            return evQuery(ev => ev.SeriesID == null);
        }

        private static int findFreeEventID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.Events.Any())
                min = (from ev in ctx.Events select ev.EventID).Min();
            return (min > -1) ? -1 : min - 1;
        }

        public void addOrUpdateEvent(Event ev)
        {
            using (var ctx = new DiversityDataContext())
            {
                if (ev.IsModified == null)
                    ev.EventID = findFreeEventID(ctx);
                ev.LogUpdatedWhen = DateTime.Now;
                ctx.Events.InsertOnSubmit(ev);
                ctx.SubmitChanges();
            }
        }
        #endregion

        #region CollectionEventProperties

        private IList<CollectionEventProperty> cepQuery(Expression<Func<CollectionEventProperty, bool>> restriction = null)
        {
            return cachedSingleKeyedQuery(ctx =>
            {
                if (restriction == null)
                    return (from cep in ctx.CollectionEventProperties
                            select cep);
                else
                    return (from cep in ctx.CollectionEventProperties
                            select cep).Where(restriction);
            },
                cep => cep.PropertyID);
        }

        public IList<CollectionEventProperty> getPropertiesForEvent(Event ev)
        {
            return cepQuery(cep => cep.EventID == ev.EventID);
        }

        public void addOrUpdateCollectionEventProperty(CollectionEventProperty cep)
        {
            using (var ctx = new DiversityDataContext())
            {
                cep.LogUpdatedWhen = DateTime.Now;
                ctx.CollectionEventProperties.InsertOnSubmit(cep);
                ctx.SubmitChanges();
            }
        }

        public IList<Property> getAllProperties()
        {

            var ctx = new DiversityDataContext();
            return new LightList<Property>(ctx.Properties);
        }

        public Property getPropertyByID(int id)
        {

            var ctx = new DiversityDataContext();
            IList<Property> propertyList= new LightList<Property>(from prop in ctx.Properties
                                           where prop.PropertyID == id
                                           select prop);
            if (propertyList.Count == 0)
                throw new KeyNotFoundException("No property with id: " + id);
            else if (propertyList.Count > 1)
                throw new Utility.PrimaryKeyViolationException("Multiple values for id: " + id);
            else return propertyList[0];
        }
        #endregion


        #region Specimen
        private IList<Specimen> specQuery(Expression<Func<Specimen, bool>> restriction = null)
        {
            return cachedSingleKeyedQuery(ctx =>
            {
                var q = (from spec in ctx.Specimen
                         select spec);
                if (restriction == null)
                    return q;
                else
                    return q.Where(restriction);
            },
                spec => spec.CollectionSpecimenID);
        }
        public IList<Specimen> getAllSpecimen()
        {
            return specQuery();
        }

        public IList<Specimen> getSpecimenForEvent(Event ev)
        {
            return specQuery(spec => spec.CollectionEventID == ev.EventID);
        }

        public IList<Specimen> getSpecimenWithoutEvent()
        {
            return specQuery(spec => spec.CollectionEventID == null);
        }

        private static int findFreeSpecimenID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.Specimen.Any())
                min = (from spec in ctx.Specimen select spec.CollectionSpecimenID).Min();
            return (min > -1) ? -1 : min - 1;
        }

        public void addOrUpdateSpecimen(Specimen spec)
        {
            using (var ctx = new DiversityDataContext())
            {
                if (spec.IsModified == null)
                {
                    spec.CollectionSpecimenID = findFreeSpecimenID(ctx);
                    ctx.Specimen.InsertOnSubmit(spec);
                }
                
                ctx.SubmitChanges();
            }
        }

        #endregion

        #region IdentificationUnit

        private IList<IdentificationUnit> iuQuery(Expression<Func<IdentificationUnit, bool>> restriction = null)
        {
            return cachedSingleKeyedQuery(ctx =>
            {
                if (restriction == null)
                    return (from iu in ctx.IdentificationUnits
                            select iu);
                else
                    return (from iu in ctx.IdentificationUnits
                            select iu).Where(restriction);
            },
                iu => iu.UnitID);
        }

        public IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec)
        {
            return iuQuery(iu => iu.SpecimenID == spec.CollectionSpecimenID && iu.RelatedUnitID == null);            
        }


        public IList<IdentificationUnit> getSubUnits(IdentificationUnit unit)
        {
            return iuQuery(iu => iu.RelatedUnitID == unit.UnitID);
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
                if (iu.IsModified == null)
                    iu.UnitID = findFreeUnitID(ctx);
                ctx.IdentificationUnits.InsertOnSubmit(iu);
                ctx.SubmitChanges();
            }
        }

        #endregion

        #region Analyses

        private IList<IdentificationUnitAnalysis> iuanQuery(Expression<Func<IdentificationUnitAnalysis, bool>> restriction = null)
        {
            return cachedSingleKeyedQuery(ctx =>
            {
                if (restriction == null)
                    return (from iuan in ctx.IdentificationUnitAnalyses
                            select iuan);
                else
                    return (from iuan in ctx.IdentificationUnitAnalyses
                            select iuan).Where(restriction);
            },
                iuan => iuan.IdentificationUnitAnalysisID);
        }


        public IList<IdentificationUnitAnalysis> getIUAForIU(IdentificationUnit iu)
        {
            return iuanQuery(iuan => iuan.IdentificationUnitID == iu.UnitID);
        }

        private static int findFreeIUAnalysisID(DiversityDataContext ctx)
        {
            int min = -1;
            if (ctx.IdentificationUnitAnalyses.Any())
                min = (from iua in ctx.IdentificationUnitAnalyses select iua.IdentificationUnitAnalysisID).Min();
            return (min > -1) ? -1 : min - 1;
        }

        public void addOrUpdateIUA(IdentificationUnitAnalysis iua)
        {
            using (var ctx = new DiversityDataContext())
            {
                if (iua.IsModified == null)
                    iua.IdentificationUnitAnalysisID = findFreeIUAnalysisID(ctx);
                ctx.IdentificationUnitAnalyses.InsertOnSubmit(iua);
                ctx.SubmitChanges();
            }
        }

        private IList<Analysis> anQuery(Expression<Func<Analysis, bool>> restriction = null)
        {
            return cachedSingleKeyedQuery(ctx =>
            {
                if (restriction == null)
                    return (from an in ctx.Analyses
                            select an);
                else
                    return (from an in ctx.Analyses
                            select an).Where(restriction);
            },
                an => an.AnalysisID);
        }

        public IList<Analysis> getAllAnalyses()
        {
            return anQuery();
        }

        public IList<Analysis> getPossibleAnalyses(string taxonomicGroup)
        {
            return cachedSingleKeyedQuery(ctx =>
                from an in ctx.Analyses
                join atg in ctx.AnalysisTaxonomicGroups on an.AnalysisID equals atg.AnalysisID
                where atg.TaxonomicGroup == taxonomicGroup
                select an,
                an => an.AnalysisID);
        }

        public Analysis getAnalysisByID(int id)
        {
            Analysis result = null;
            withDataContext((ctx) =>
                {
                    result = (from an in ctx.Analyses
                              where an.AnalysisID == id
                              select an).FirstOrDefault();
                });
            return result;
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
            return cachedSingleKeyedQuery(ctx =>            
                from ar in ctx.AnalysisResults
                where ar.AnalysisID == analysisID
                select ar,
                ar => ar.Result.GetHashCode());
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

        public IList<MultimediaObject> getMultimediaForEventSeries(EventSeries es)
        {
            return uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.SourceId == (int)SourceID.EventSeries
                                                && mm.RelatedId == es.SeriesID
                                        select mm);
        }

      
        public IList<MultimediaObject> getMultimediaForEvent(Event ev)
        {
            return uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.SourceId == (int)SourceID.Event
                                                && mm.RelatedId == ev.EventID
                                        select mm);            
        }


        public IList<MultimediaObject> getMultimediaForSpecimen(Specimen spec)
        {
            return uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.SourceId == (int)SourceID.Specimen
                                                && mm.RelatedId == spec.CollectionSpecimenID
                                        select mm);
        }

        public IList<MultimediaObject> getMultimediaForIdentificationUnit(IdentificationUnit iu)
        {
            return uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.SourceId == (int)SourceID.IdentificationUnit
                                                && mm.RelatedId == iu.UnitID
                                        select mm);
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

        public IList<Term> getTerms(int source)
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
                ctx.SubmitChanges();
            }

            sampleData();
        }

        #endregion

        #region TaxonNames

        public void addTaxonNames(IEnumerable<TaxonName> taxa, int tableID)
        {
            using (var ctx = new DiversityDataContext())
            {
                switch (tableID)
                {
                    case 0: ctx.TaxonNames0.InsertAllOnSubmit(taxa);
                        break;
                    case 1: ctx.TaxonNames1.InsertAllOnSubmit(taxa);
                        break;
                    case 2: ctx.TaxonNames2.InsertAllOnSubmit(taxa);
                        break;
                    case 3: ctx.TaxonNames3.InsertAllOnSubmit(taxa);
                        break;
                    case 4: ctx.TaxonNames4.InsertAllOnSubmit(taxa);
                        break;
                    case 5: ctx.TaxonNames5.InsertAllOnSubmit(taxa);
                        break;
                    case 6: ctx.TaxonNames6.InsertAllOnSubmit(taxa);
                        break;
                    case 7: ctx.TaxonNames7.InsertAllOnSubmit(taxa);
                        break;
                    case 8: ctx.TaxonNames8.InsertAllOnSubmit(taxa);
                        break;
                    case 9: ctx.TaxonNames9.InsertAllOnSubmit(taxa);
                        break;
                    default:
                        throw new IndexOutOfRangeException("Only 10 tables are supported. Id is not between 0 and 9");
                }
                ctx.SubmitChanges();
            }
        }

        private Table<TaxonName> getTaxonTable(int tableID,DiversityDataContext ctx)
        {
            switch (tableID)
                {
                    case 0: return ctx.TaxonNames0;
                        break;
                    case 1: return ctx.TaxonNames1;
                        break;
                    case 2: return ctx.TaxonNames2;
                        break;
                    case 3: return ctx.TaxonNames3;
                        break;
                    case 4: return ctx.TaxonNames4;
                        break;
                    case 5: return ctx.TaxonNames5;
                        break;
                    case 6: return ctx.TaxonNames6;
                        break;
                    case 7: return ctx.TaxonNames7;
                        break;
                    case 8: return ctx.TaxonNames8;
                        break;
                    case 9: return ctx.TaxonNames9;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Only 10 tables are supported. Id is not between 0 and 9");
                }   
        }

        public IList<TaxonName> getTaxonNames(int tableID)
        {
            using (var ctx = new DiversityDataContext())
            {
                switch (tableID)
                {
                    case 0: return new LightList<TaxonName>(from taxa in ctx.TaxonNames0
                                                            select taxa);
                    case 1: return new LightList<TaxonName>(from taxa in ctx.TaxonNames1
                                                            select taxa);
                    case 2: return new LightList<TaxonName>(from taxa in ctx.TaxonNames2
                                                            select taxa);
                    case 3: return new LightList<TaxonName>(from taxa in ctx.TaxonNames3
                                                            select taxa);
                    case 4: return new LightList<TaxonName>(from taxa in ctx.TaxonNames4
                                                            select taxa);
                    case 5: return new LightList<TaxonName>(from taxa in ctx.TaxonNames5
                                                            select taxa);
                    case 6: return new LightList<TaxonName>(from taxa in ctx.TaxonNames6
                                                            select taxa);
                    case 7: return new LightList<TaxonName>(from taxa in ctx.TaxonNames7
                                                            select taxa);
                    case 8: return new LightList<TaxonName>(from taxa in ctx.TaxonNames8
                                                            select taxa);
                    case 9: return new LightList<TaxonName>(from taxa in ctx.TaxonNames9
                                                            select taxa);
                    default:
                        throw new IndexOutOfRangeException("Only 10 tables are supported. Id is not between 0 and 9");
                }
            }
        }

        public IList<TaxonName> getTaxonNames(Term taxonGroup)
        {
            var ctx = new DiversityDataContext();
            IList <TaxonSelection> selection= new LightList<TaxonSelection>(from ts in ctx.taxonSelection
                                                                    where ts.taxonomicGroup.Equals(taxonGroup.Code)
                                                                    && ts.isSelected == true
                                                                    select ts);
            if (selection.Count == 0)
                throw new KeyNotFoundException();
            else if (selection.Count > 1)
                throw new PrimaryKeyViolationException();
            TaxonSelection selected = selection[0];
            return this.getTaxonNames(selected.tableID);
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
            var ctx = new DiversityDataContext();
            return new LightList<PropertyName>(from props in ctx.PropertyNames
                                               where props.PropertyID==prop.PropertyID
                                            select props);
        }

        public PropertyName getPropertyNameByURI(string uri)
        {
            var ctx = new DiversityDataContext();
            LightList<PropertyName> propNameList = new LightList<PropertyName>(from propName in ctx.PropertyNames
                                                                            where propName.PropertyUri.Equals(uri)
                                                                            select propName);
            if (propNameList.Count == 0)
                throw new KeyNotFoundException("No propertyName with uri: " + uri);
            else if (propNameList.Count > 1)
                throw new Utility.PrimaryKeyViolationException("Multiple values for id: " + uri);
            else return propNameList[0];
        }

        #endregion

        #region Maps

        public IList<Map> getAllMaps()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Map>(from maps in ctx.Maps
                                            select maps);
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
            using (var ctx = new DiversityDataContext())
            {
                ctx.EventSeries.InsertOnSubmit(new Model.EventSeries() { SeriesID = 1, Description = "ES" });
                ctx.Events.InsertOnSubmit(new Model.Event() { SeriesID = 0, EventID = 0, LocalityDescription = "EV" });
                ctx.Specimen.InsertOnSubmit(new Model.Specimen() { CollectionEventID = 0, CollectionSpecimenID = 0, AccesionNumber = "CS" });
                ctx.IdentificationUnits.InsertOnSubmit(new IdentificationUnit() { SpecimenID = 0, UnitID = 0 });
                int id = 1;
                recSample(0, 0, ref id, ctx);
                ctx.SubmitChanges();
            }

        }
        private void recSample(int depth, int parent, ref int id, DiversityDataContext ctx)
        {
            if (depth == 3)
                return;

            depth++;
            int p = id;


            for (int i = 0; i < 20; i++)
            {
                ctx.IdentificationUnits.InsertOnSubmit(new IdentificationUnit() { UnitID = id,  RelatedUnitID = parent });
                recSample(depth, id++, ref id, ctx);
            }
        }

        #endregion     
    
        #region Generische Implementierungen
        private void addOrUpdateRow<T>(Func<DiversityDataContext, Table<T>> tableSelector, Expression<Func<T,int>> keySelector, T row) where T : class
        {
            if(tableSelector == null)
                throw new ArgumentNullException("tableSelector");
            if(row == null)
                throw new ArgumentNullException("row");

            var equalsKeyExpression = EqualsKeyExpression(keySelector, row);

            withDataContext((ctx) =>
                {
                    var table = tableSelector(ctx);
                    var existingRow = (from element in table                                      
                                       select element)
                                       .Where(equalsKeyExpression)
                                       .FirstOrDefault();

                    if (existingRow != null)
                        table.Attach(row, existingRow);
                    else
                    {        
                        
                        table.InsertOnSubmit(row);
                    }

                    ctx.SubmitChanges();
                });
        }

        /// <summary>
        /// Returns an unused primary Key for the given Table.
        /// Returns only negative Keys
        /// </summary>
        /// <typeparam name="T">Entity Type of the Table</typeparam>
        /// <param name="table"></param>
        /// <param name="keyExpression">Expression to be used as primary Key</param>
        /// <returns>Next Lowest unused primary Key value, always negative.</returns>
        private int findFreeKey<T>(IQueryable<T> table, Expression<Func<T, int>> keyExpression)
        {
            var lowerthanMin = table.Select(keyExpression).Min() - 1;
            return (lowerthanMin < -1) ? lowerthanMin : -1;
        }

        private void withDataContext(Action<DiversityDataContext> operation)
        {
            using (var ctx = new DiversityDataContext())
                operation(ctx);
        }

        private IList<T> cachedMultiKeyQuery<T>(Func<DiversityDataContext, IQueryable<T>> oderedQuery, Expression<Func<T,T, bool>> match)
        {
            return null; //TODO
        }

        private IList<T> cachedSingleKeyedQuery<T>(Func<DiversityDataContext, IQueryable<T>> query, Expression<Func<T, int>> KeyExpression) where T : class
        {
            return new RotatingCache<T>(new SingleKeyedCacheSource<T>(query,KeyExpression));
        }

        private IList<T> uncachedQuery<T>(Func<DiversityDataContext, IQueryable<T>> query)
        {
            IList<T> result = null;
            withDataContext(ctx => result = query(ctx).ToList());
            return result;
        }


        private class SingleKeyedCacheSource<T> : ICacheSource<T>
        {
            Func<DiversityDataContext, IQueryable<T>> queryFunc;
            Expression<Func<T, int>> keyExpression;
            Func<T, int> keyFunc;

            public SingleKeyedCacheSource(Func<DiversityDataContext, IQueryable<T>> QueryFunc, Expression<Func<T,int>> KeyExpression)
            {
                queryFunc = QueryFunc;
                keyExpression = KeyExpression;
                keyFunc = keyExpression.Compile();                
            }

            public IEnumerable<T> retrieveItems(int count, int offset)
            {
                using (var ctx = new DiversityDataContext())
                {
                    return queryFunc(ctx)
                        .OrderBy(keyExpression)
                        .Skip(offset)
                        .Take(count)
                        .ToList(); //Force execution of query
                }
            }

            public int Count
            {
                get 
                {
                    using (var ctx = new DiversityDataContext())
                    {
                        return queryFunc(ctx)
                            .Count();
                    }
                }
            }
        

            public int  IndexOf(T item)
            {                
                var lessThanKeyExpression = LessThanKeyExpression(keyExpression, item);


                using (var ctx = new DiversityDataContext())
                {
                    return queryFunc(ctx)
                        .OrderBy(keyExpression)
                        .Where(lessThanKeyExpression)
                        .Count();
                }
            }        

        }

        private static Expression<Func<T, bool>> LessThanKeyExpression<T>(Expression<Func<T, int>> keyExpression, T item)
        {
            //Get the Key to look for
            var key = keyExpression.Compile()(item);
            //Build Comparison Expression "row => row.key < key"
            return Expression.Lambda<Func<T, bool>>(Expression.LessThan(keyExpression, Expression.Constant(key, typeof(int))), keyExpression.Parameters);
        }


        private static Expression<Func<T, bool>> EqualsKeyExpression<T>(Expression<Func<T, int>> keyExpression, T item)
        {
            //Get the Key to look for
            var key = keyExpression.Compile()(item);
            //Build Comparison Expression "row => row.key == key"
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(keyExpression, Expression.Constant(key, typeof(int))), keyExpression.Parameters);
        }
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
