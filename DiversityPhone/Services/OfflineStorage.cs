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
    using System.IO.IsolatedStorage;

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
                _messenger.Listen<Event>(MessageContracts.DELETE)
                    .Subscribe(ev=>deleteEvent(ev)),
                _messenger.Listen<CollectionEventProperty>(MessageContracts.SAVE)
                    .Subscribe(cep=>addOrUpdateCollectionEventProperty(cep)),
                _messenger.Listen<Specimen>(MessageContracts.SAVE)
                    .Subscribe(spec => addOrUpdateSpecimen(spec)),
                _messenger.Listen<Specimen>(MessageContracts.DELETE)
                    .Subscribe(spec=>deleteSpecimen(spec)),
                _messenger.Listen<IdentificationUnit>(MessageContracts.SAVE)
                    .Subscribe(iu => addOrUpdateIUnit(iu)),
                _messenger.Listen<IdentificationUnit>(MessageContracts.DELETE)
                    .Subscribe(iu=>deleteIU(iu)),
                _messenger.Listen<IdentificationUnitAnalysis>(MessageContracts.SAVE)
                    .Subscribe(iua=>addOrUpdateIUA(iua)),
                 _messenger.Listen<IdentificationUnitAnalysis>(MessageContracts.DELETE)
                    .Subscribe(iua=>deleteIUA(iua)),
                _messenger.Listen<MultimediaObject>(MessageContracts.SAVE)
                    .Subscribe(mmo => addMultimediaObject(mmo)),
                _messenger.Listen<MultimediaObject>(MessageContracts.DELETE)
                    .Subscribe(mmo=>deleteMMO(mmo)),

                _messenger.Listen<Term>(MessageContracts.USE)
                    .Subscribe(term => updateLastUsed(term)),
            };

            using (var context = new DiversityDataContext())
            {
                if (!context.DatabaseExists())
                {
                   context.CreateDatabase();
                   
                }
            }
        }

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

        public void addOrUpdateEventSeries(EventSeries newSeries)
        {
            if (EventSeries.isNoEventSeries(newSeries))
                return;
            addOrUpdateRow(EventSeries.Operations, ctx => ctx.EventSeries, newSeries);            
        }

        public void deleteEventSeries(EventSeries toDeleteEs)
        {
            IList<Event> attachedEvents = this.getEventsForSeries(toDeleteEs);
            foreach (Event ev in attachedEvents)
            {
                this.deleteEvent(ev);
            }
            IList<MultimediaObject> attachedMMO = this.getMultimediaForObject(ReferrerType.EventSeries, toDeleteEs.SeriesID);
            foreach(MultimediaObject mmo in attachedMMO)
            {
                this.deleteMMO(mmo);
            }
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

            if (ev.SeriesID != null)
            {
                EventSeries es = this.getEventSeriesByID((int)ev.SeriesID);
                if (es.DiversityCollectionEventSeriesID != null)
                    ev.DiversityCollectionSeriesID = es.DiversityCollectionEventSeriesID;
            }


            addOrUpdateRow(Event.Operations,
                  ctx => ctx.Events,
                  ev
              );

            if (wasNewEvent)
            {
                Specimen observation = new Specimen().MakeObservation();
                observation.CollectionEventID = ev.EventID;
                addOrUpdateSpecimen(observation);

                //withDataContext((ctx) =>{
                ////Now EventID is set even for new Events
                //Specimen observation = new Specimen().MakeObservation();
                //observation.CollectionEventID = ev.EventID;
                //ev.Specimen.Add(observation);
                //ctx.SubmitChanges();
                //});
            }
        }

        public void deleteEvent(Event toDeleteEv)
        {
            //withDataContext((ctx) =>
            //     {
            //         IList<Specimen> attachedSpecimen = this.getSpecimenForEvent(toDeleteEv);
                     
            //         foreach (Specimen spec in attachedSpecimen)
            //         {
            //             toDeleteEv.Specimen.Add(spec);
            //         }
            //         ctx.Events.DeleteOnSubmit(toDeleteEv);
            //         ctx.SubmitChanges();
            //     });
            IList<Specimen> attachedSpecimen = this.getSpecimenForEvent(toDeleteEv);
            foreach (Specimen spec in attachedSpecimen)
            {
                this.deleteSpecimen(spec);
            }
            IList<CollectionEventProperty> attachedProperties = this.getPropertiesForEvent(toDeleteEv);
            foreach (CollectionEventProperty cep in attachedProperties)
                this.deleteEventProperty(cep);
            IList<MultimediaObject> attachedMMO = this.getMultimediaForObject(ReferrerType.Event, toDeleteEv.EventID);
            foreach (MultimediaObject mmo in attachedMMO)
            {
                this.deleteMMO(mmo);
            }
            deleteRow(Event.Operations, ctx => ctx.Events, toDeleteEv);
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
            Event ev = this.getEventByID(cep.EventID);
            if (ev.DiversityCollectionEventID != null)
                cep.DiversityCollectionEventID = ev.DiversityCollectionEventID;

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

        public void deleteEventProperty(CollectionEventProperty toDeleteCep)
        {
            deleteRow(CollectionEventProperty.Operations, ctx => ctx.CollectionEventProperties, toDeleteCep);
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
            IList<Specimen> specList= cachedQuery(Specimen.Operations,
            ctx =>
                from spec in ctx.Specimen                 
                where spec.CollectionEventID == ev.EventID
                select spec
                );
           
            return specList;
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
            Event ev = this.getEventByID(spec.CollectionEventID);
            if (ev.DiversityCollectionEventID != null)
                spec.DiversityCollectionEventID = ev.DiversityCollectionEventID;
            addOrUpdateRow(Specimen.Operations,
                ctx => ctx.Specimen,
                spec
            );
        }

        public void deleteSpecimen(Specimen toDeleteSpec)
        {
            IList<MultimediaObject> attachedMMO = this.getMultimediaForObject(ReferrerType.Specimen, toDeleteSpec.CollectionSpecimenID);
            foreach (MultimediaObject mmo in attachedMMO)
            {
                this.deleteMMO(mmo);
            }
            IList<IdentificationUnit> attachedTopLevelIU = this.getTopLevelIUForSpecimen(toDeleteSpec);
            foreach (IdentificationUnit topIU in attachedTopLevelIU)
                this.deleteIU(topIU);
            deleteRow(Specimen.Operations, ctx => ctx.Specimen, toDeleteSpec);
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

        public void addOrUpdateIUnit(IdentificationUnit iu)
        {
            Specimen spec = this.getSpecimenByID(iu.SpecimenID);
            if (spec.DiversityCollectionSpecimenID != null)
                iu.DiversityCollectionSpecimenID = spec.DiversityCollectionSpecimenID;

            if (iu.RelatedUnitID != null)
            {
                IdentificationUnit relatedIU = this.getIdentificationUnitByID((int) iu.RelatedUnitID);
                if (relatedIU.DiversityCollectionUnitID != null)
                    iu.DiversityCollectionRelatedUnitID = relatedIU.DiversityCollectionUnitID;

            }
            addOrUpdateRow(IdentificationUnit.Operations, ctx => ctx.IdentificationUnits, iu);           
        }

        public void deleteIU(IdentificationUnit toDeleteIU)
        {
            IList<MultimediaObject> attachedMMO = this.getMultimediaForObject(ReferrerType.IdentificationUnit, toDeleteIU.UnitID);
            foreach (MultimediaObject mmo in attachedMMO)
            {
                this.deleteMMO(mmo);
            }
            IList<IdentificationUnit> attachedUnits = this.getSubUnits(toDeleteIU);
            foreach (IdentificationUnit iu in attachedUnits)
                this.deleteIU(iu);
            IList<IdentificationUnitAnalysis> attachedAnalyses = this.getIUANForIU(toDeleteIU);
            foreach (IdentificationUnitAnalysis iua in attachedAnalyses)
                this.deleteIUA(iua);
            deleteRow(IdentificationUnit.Operations, ctx => ctx.IdentificationUnits, toDeleteIU);
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
            IdentificationUnit iu = this.getIdentificationUnitByID(iua.IdentificationUnitID);
            if (iu.DiversityCollectionUnitID != null)
                iua.DiversityCollectionUnitID = iu.DiversityCollectionUnitID;
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
        //This query can't be (unordered join) and doesn't have to be (very small) cached 
            return uncachedQuery(ctx =>
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
            return uncachedQuery(ctx =>
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
                try
                {
                    ctx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debugger.Break();
                }
                
            }
        }

        public void deleteIUA(IdentificationUnitAnalysis toDeleteIUA)
        {
            deleteRow(IdentificationUnitAnalysis.Operations, ctx => ctx.IdentificationUnitAnalyses, toDeleteIUA);
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
            switch (mmo.OwnerType)
            {
                case ReferrerType.EventSeries:
                    EventSeries es = this.getEventSeriesByID(mmo.RelatedId);
                    if (es.DiversityCollectionEventSeriesID != null)
                        mmo.DiversityCollectionRelatedID = es.DiversityCollectionEventSeriesID;
                    break;
                case ReferrerType.Event:
                    Event ev = this.getEventByID(mmo.RelatedId);
                    if (ev.DiversityCollectionEventID != null)
                        mmo.DiversityCollectionRelatedID = ev.DiversityCollectionEventID;
                    break;
                case ReferrerType.Specimen:
                    Specimen spec = this.getSpecimenByID(mmo.RelatedId);
                    if (spec.DiversityCollectionSpecimenID != null)
                        mmo.DiversityCollectionRelatedID = spec.DiversityCollectionSpecimenID;
                    break;
                case ReferrerType.IdentificationUnit:
                    IdentificationUnit iu = this.getIdentificationUnitByID(mmo.RelatedId);
                    if (iu.DiversityCollectionUnitID != null)
                        mmo.DiversityCollectionRelatedID = iu.DiversityCollectionUnitID;
                    break;
                default:
                    break;
            }
            using (var ctx = new DiversityDataContext())
            {
                ctx.MultimediaObjects.InsertOnSubmit(mmo);
                ctx.SubmitChanges();
            }
        }

        public void deleteMMO(MultimediaObject toDeleteMMO)
        {
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(toDeleteMMO.Uri))
            {
                myStore.DeleteFile(toDeleteMMO.Uri);
            }
            deleteRow(MultimediaObject.Operations, ctx => ctx.MultimediaObjects, toDeleteMMO);
        }


        #endregion

        #region Terms

        public IList<Term> getTerms(Svc.TermList source)
        {
            return uncachedQuery(ctx => from t in ctx.Terms
                                        where t.SourceID == source
                                        orderby t.LastUsed descending
                                        select t
                                        );
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
                    System.Diagnostics.Debugger.Break();
                    //TODO Log
                }
                 
            }
            sampleData();
        }

        public void updateLastUsed(Term term)
        {
            if (term == null)
            {
#if DEBUG
                throw new ArgumentNullException("term");
#else
                return;
#endif
                //TODO Log
            }

            withDataContext(ctx =>
            {
                ctx.Terms.Attach(term);
                term.LastUsed = DateTime.Now;
                ctx.SubmitChanges();
            });
        }

        #endregion

        #region TaxonNames

        public void addTaxonNames(IEnumerable<TaxonName> taxa, Svc.TaxonList list)
        {
            int tableIdx = -1;
            lock (this)
            {                
                withDataContext(ctx =>
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
            withDataContext(ctx =>
                {
                    result = (ctx.TaxonSelection.ToList());
                });
            return result;
        }

        public void selectTaxonList(Svc.TaxonList list)
        {
            withDataContext(ctx =>
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
            withDataContext(ctx =>
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
            withDataContext(ctx =>
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

        private IEnumerable<int> getUnusedTaxonTableIDs(DiversityDataContext ctx)
        {
            var usedTableIDs = from ts in ctx.TaxonSelection
                               select ts.TableID;
            return TaxonSelection.ValidTableIDs.Except(usedTableIDs);           
        }    

        private IList<TaxonName> getTaxonNames(int tableID, string query)
        {
            var queryWords = query.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);

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
            if(taxonGroup != null)
                withDataContext(ctx =>
                {
                    var assignment = from a in ctx.TaxonSelection
                                     where a.TaxonomicGroup == taxonGroup && a.IsSelected
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
                //TODO Log
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
                                    try
                                    {
                                        ctx2.SubmitChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debugger.Break();
                                    }
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

        /// <summary>
        /// Creates s Tree of IModifyable Objects with ModificationState==true with an Event as the root.
        /// It has to be searched the complete Specimen and IdentificationUnitLevel of the event as there can be changes on these objects even when the event or corresponding epcimen,iu is unchanged.
        /// </summary>
        public Svc.HierarchySection getNewHierarchyToSyncBelow(Event ev) 
        {
            Svc.HierarchySection result = new Svc.HierarchySection();
            result.Properties = new System.Collections.ObjectModel.ObservableCollection<Svc.CollectionEventProperty>();
            result.Specimen = new System.Collections.ObjectModel.ObservableCollection<Svc.Specimen>();
            result.IdentificationUnits = new System.Collections.ObjectModel.ObservableCollection<Svc.IdentificationUnit>();
            result.IdentificationUnitAnalyses = new System.Collections.ObjectModel.ObservableCollection<Svc.IdentificationUnitAnalysis>();
            if (ev.IsModified())
                result.Event = Event.ConvertToServiceObject(ev);

            withDataContext(ctx =>
            {

                IQueryable<CollectionEventProperty> clientPropertyList =
                    from cep in ctx.CollectionEventProperties
                    where cep.EventID == ev.EventID && cep.ModificationState == true
                    select cep;
                foreach (CollectionEventProperty cep in clientPropertyList)
                {
                    Svc.CollectionEventProperty serverCep = CollectionEventProperty.ConvertToServiceObject(cep);
                    result.Properties.Add(serverCep);
                }

                IQueryable<Specimen> clientSpecList =
                    from spec in ctx.Specimen
                    where spec.CollectionEventID == ev.EventID
                    select spec;
                foreach (Specimen spec in clientSpecList)
                {
                    if (spec.ModificationState == true)
                    {
                        Svc.Specimen serverSpec = Specimen.ConvertToServiceObject(spec);
                        result.Specimen.Add(serverSpec);
                    }
                    IQueryable<IdentificationUnit> clientIUListForSpec =
                        from iu in ctx.IdentificationUnits
                        where iu.UnitID == spec.CollectionSpecimenID
                        select iu;
                    foreach (IdentificationUnit iu in clientIUListForSpec)
                    {
                        if (iu.ModificationState == true)
                        {
                            Svc.IdentificationUnit serverIU = IdentificationUnit.ConvertToServiceObject(iu);
                            result.IdentificationUnits.Add(serverIU);
                        }

                        IQueryable<IdentificationUnitAnalysis> clientIUAListForIU =
                            from iua in ctx.IdentificationUnitAnalyses
                            where iua.IdentificationUnitID == iu.UnitID && iu.ModificationState == true
                            select iua;
                        foreach (IdentificationUnitAnalysis iua in clientIUAListForIU)
                        {
                            Svc.IdentificationUnitAnalysis serverIUA = IdentificationUnitAnalysis.ConvertToServiceObject(iua);
                            result.IdentificationUnitAnalyses.Add(serverIUA);
                        }
                    }
                }
            });
            return result;
        }

        public IList<EventSeries> getUploadServiceEventSeries()
        {
            IList<EventSeries> res = null;
            withDataContext(ctx =>
            {
               var query = from es in ctx.EventSeries
                           where es.ModificationState == true
                           select es;
               res = query.ToList();               
            });
            return res;
        }

        public void updateHierarchy(Svc.HierarchySection from, Svc.HierarchySection to)
        {
            throw new NotImplementedException();
        }

        #region KeyUpdate

        public void updateSeriesKey(int clientSeriesKey, int serverSeriesKey)
        {
            using (DiversityDataContext ctx = new DiversityDataContext())
            {
                var savedSeries =
                    from es in ctx.EventSeries
                    where es.SeriesID == clientSeriesKey
                    select es;
                EventSeries clientSeries = savedSeries.First(); //TODO: Check if there is a key valuation
                clientSeries.DiversityCollectionEventSeriesID = serverSeriesKey;
                clientSeries.ModificationState = false;
                var savedEvents =
                    from ev in ctx.Events
                    where ev.SeriesID == clientSeriesKey
                    select ev;
                foreach (Event eve in savedEvents)
                    eve.DiversityCollectionSeriesID = serverSeriesKey;
                var seriesMMO =
                    from mmo in ctx.MultimediaObjects
                    where mmo.RelatedId == clientSeriesKey && mmo.OwnerType == ReferrerType.EventSeries
                    select mmo;
                foreach (MultimediaObject mmo in seriesMMO)
                    mmo.DiversityCollectionRelatedID = serverSeriesKey;
                ctx.SubmitChanges();
            }
        }

        public void updateEventKey(int clientKey, int serverKey)
        {
            using (DiversityDataContext ctx = new DiversityDataContext())
            {
                var savedEvents =
                    from ev in ctx.Events
                    where ev.EventID == clientKey
                    select ev;
                Event clientEvent = savedEvents.First();//TODO: Check if there is a key valuation
                clientEvent.DiversityCollectionEventID = serverKey;
                clientEvent.ModificationState = false;
                var savedSpecimen =
                    from spec in ctx.Specimen
                    where spec.CollectionEventID == clientKey
                    select spec;
                foreach (Specimen spec in savedSpecimen)
                    spec.DiversityCollectionEventID = serverKey;
                var evMMO =
                    from mmo in ctx.MultimediaObjects
                    where mmo.RelatedId == clientKey && mmo.OwnerType == ReferrerType.Event
                    select mmo;
                foreach (MultimediaObject mmo in evMMO)
                    mmo.DiversityCollectionRelatedID = serverKey;
                var ceProperties =
                    from cep in ctx.CollectionEventProperties
                    where cep.EventID == clientKey
                    select cep;
                foreach (CollectionEventProperty cep in ceProperties)
                    cep.DiversityCollectionEventID = serverKey;
                ctx.SubmitChanges();
            }
        }

        public void updateSpecimenKey(int clientKey, int serverKey)
        {
            using (DiversityDataContext ctx = new DiversityDataContext())
            {
                var savedSpecimens =
                    from spec in ctx.Specimen
                    where spec.CollectionSpecimenID == clientKey
                    select spec;
                Specimen clientSpecimen = savedSpecimens.First();//TODO: Check if there is a key valuation
                clientSpecimen.DiversityCollectionSpecimenID = serverKey;
                clientSpecimen.ModificationState = false;
                var savedIU =
                    from iu in ctx.IdentificationUnits
                    where iu.SpecimenID == clientKey
                    select iu;
                foreach (IdentificationUnit iu in savedIU)
                    iu.DiversityCollectionSpecimenID = serverKey;
                var specMMO =
                    from mmo in ctx.MultimediaObjects
                    where mmo.RelatedId == clientKey && mmo.OwnerType == ReferrerType.Specimen
                    select mmo;
                foreach (MultimediaObject mmo in specMMO)
                    mmo.DiversityCollectionRelatedID = serverKey;
                ctx.SubmitChanges();
            }
        }

        public void updateIUKey(int clientKey, int serverKey) 
        {
            using (DiversityDataContext ctx = new DiversityDataContext())
            {
                var savedIUs =
                    from iu in ctx.IdentificationUnits
                    where iu.UnitID == clientKey
                    select iu;
                IdentificationUnit clientIU = savedIUs.First();//TODO: Check if there is a key valuation
                clientIU.DiversityCollectionUnitID= serverKey;
                clientIU.ModificationState = false;
                var relatedIU =
                    from iu in ctx.IdentificationUnits
                    where iu.RelatedUnitID == clientKey
                    select iu;
                foreach (IdentificationUnit iu in relatedIU)
                    iu.DiversityCollectionRelatedUnitID = serverKey;
                var iuaList =
                    from iua in ctx.IdentificationUnitAnalyses
                    where iua.IdentificationUnitID == clientKey
                    select iua;
                foreach (IdentificationUnitAnalysis iua in iuaList)
                    iua.DiversityCollectionUnitID = serverKey;
                var iuMMO =
                    from mmo in ctx.MultimediaObjects
                    where mmo.RelatedId == clientKey && mmo.OwnerType == ReferrerType.IdentificationUnit
                    select mmo;
                foreach (MultimediaObject mmo in iuMMO)
                    mmo.DiversityCollectionRelatedID = serverKey;
                ctx.SubmitChanges();
            }
        }

        #endregion

        #endregion



        public void clearDatabase()
        {
            using (var context = new DiversityDataContext())
            {
                context.DeleteDatabase();
                context.CreateDatabase();
            }
        }
    }
}
