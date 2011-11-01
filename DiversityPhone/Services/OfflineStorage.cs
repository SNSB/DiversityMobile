namespace DiversityPhone.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using ReactiveUI;
    using DiversityPhone.Messages;
    using DiversityPhone.Utility;

    public class OfflineStorage : IOfflineStorage
    {
        private IList<IDisposable> _subscriptions;
        private IMessageBus _messenger;

        public OfflineStorage(IMessageBus messenger)
        {
            this._messenger = messenger;

            _subscriptions = new List<IDisposable>()
            {
                _messenger.Listen<Event>(MessageContracts.SAVE)
                    .Subscribe(ev => addOrUpdateEvent(ev)),

                _messenger.Listen<EventSeries>(MessageContracts.SAVE)
                    .Subscribe(es => addOrUpdateEventSeries(es)),

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

        #region EventSeries

        public IList<EventSeries> getAllEventSeries()
        {
            var ctx = new DiversityDataContext();
            return new LightList<EventSeries>(ctx.EventSeries);
        }


        public EventSeries getEventSeriesByID(int id)
        {
            var ctx = new DiversityDataContext();
            LightList<EventSeries> esList= new LightList<EventSeries>(from es in ctx.EventSeries
                                        where es.SeriesID == id
                                        select es);
            if (esList.Count == 0)
                throw new KeyNotFoundException("No series with ID: " + id);
            else if (esList.Count > 1)
                throw new Utility.PrimaryKeyViolationException("Multiple values for id: " + id);
            else return esList[0];
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
            if (newSeries.IsModified != null || newSeries.SeriesID != default(int))
                throw new InvalidOperationException("Series is not new!");

            using (var ctx = new DiversityDataContext())
            {
                newSeries.SeriesID = findFreeEventSeriesID(ctx);
                newSeries.LogUpdatedWhen = DateTime.Now;
                ctx.EventSeries.InsertOnSubmit(newSeries);
                ctx.SubmitChanges();
            }
        }
        #endregion

        #region Event
        public IList<Event> getAllEvents()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Event>(ctx.Events);

        }


        public IList<Event> getEventsForSeries(EventSeries es)
        {
            var ctx = new DiversityDataContext();
            return new LightList<Event>(from e in ctx.Events
                                        where e.SeriesID == es.SeriesID
                                        select e);
        }

        public IList<Event> getEventsWithoutSeries()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Event>(from e in ctx.Events
                                        where e.SeriesID == null
                                        select e);
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

        public IList<CollectionEventProperty> getPropertiesForEvent(Event ev)
        {
            var ctx = new DiversityDataContext();
            return new LightList<CollectionEventProperty>(from cep in ctx.CollectionEventProperties
                                        where cep.EventID == ev.EventID
                                        select cep);
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


        #endregion


        #region Specimen


        public IList<Specimen> getAllSpecimen()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Specimen>(ctx.Specimen);
        }

        public IList<Specimen> getSpecimenForEvent(Event ev)
        {
            var ctx = new DiversityDataContext();
            return new LightList<Specimen>(from spec in ctx.Specimen
                                           where spec.CollectionEventID == ev.EventID
                                           select spec);
        }

        public IList<Specimen> getSpecimenWithoutEvent()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Specimen>(from spec in ctx.Specimen
                                        where spec.CollectionEventID == null
                                        select spec);
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
                    spec.CollectionSpecimenID = findFreeSpecimenID(ctx);
                ctx.Specimen.InsertOnSubmit(spec);
                ctx.SubmitChanges();
            }
        }

        #endregion

        #region IdentificationUnit

        public IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec)
        {
            var ctx = new DiversityDataContext();
            return new LightList<IdentificationUnit>(from iu in ctx.IdentificationUnits
                                                     where iu.SpecimenID == spec.CollectionSpecimenID && iu.RelatedUnitID == null
                                                     select iu);
        }


        public IList<IdentificationUnit> getSubUnits(IdentificationUnit unit)
        {
            var ctx = new DiversityDataContext();
            return new LightList<IdentificationUnit>(from iu in ctx.IdentificationUnits
                                                     where iu.RelatedUnitID == unit.UnitID
                                                     select iu);
        }

        public IdentificationUnit getIUbyID(int id)
        {
            var ctx = new DiversityDataContext();
            LightList<IdentificationUnit> iuList = new LightList<IdentificationUnit>(from iu in ctx.IdentificationUnits
                                                                       where iu.UnitID == id
                                                                       select iu);
            if (iuList.Count == 0)
                throw new KeyNotFoundException("No IU with ID: " + id);
            else if (iuList.Count > 1)
                throw new Utility.PrimaryKeyViolationException("Multiple values for id: " + id);
            else return iuList[0];
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

        public IList<IdentificationUnitAnalysis> getIUAForIU(IdentificationUnit iu)
        {
            var ctx = new DiversityDataContext();
            return new LightList<IdentificationUnitAnalysis>(from iua in ctx.IdentificationUnitAnalyses
                                                     where iua.IdentificationUnitID == iu.UnitID
                                                     select iua);
        }

        private static int findFreeAnalysisID(DiversityDataContext ctx)
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
                    iua.IdentificationUnitAnalysisID = findFreeAnalysisID(ctx);
                ctx.IdentificationUnitAnalyses.InsertOnSubmit(iua);
                ctx.SubmitChanges();
            }
        }

        public IList<Analysis> getAllAnalyses()
        {
            var ctx = new DiversityDataContext();
            return new LightList<Analysis>(ctx.Analyses);
        }

        public IList<Analysis> getPossibleAnalyses(string taxonomicGroup)
        {
            IList<AnalysisTaxonomicGroup> allowed = this.getAnalysisTaxonomicGroups(taxonomicGroup);
            IList<Analysis> all = this.getAllAnalyses();
            IList<Analysis> possible = new List<Analysis>();
            foreach (Analysis ana in all)
            {
                foreach (AnalysisTaxonomicGroup atg in allowed)
                {
                    if (ana.AnalysisID == atg.AnalysisID)
                    {
                        possible.Add(ana);
                        break;
                    }
                }
            }
            return possible;
        }

        public Analysis getAnalysis(int analysisID)
        {
            var ctx = new DiversityDataContext();
            LightList<Analysis> anaList = new LightList<Analysis>(from ana in ctx.Analyses
                                                                       where ana.AnalysisID == analysisID
                                                                       select ana);
            if (anaList.Count == 0)
                throw new KeyNotFoundException("No analysis with ID: " + analysisID);
            else if (anaList.Count > 1)
                throw new Utility.PrimaryKeyViolationException("Multiple values for id: " + analysisID);
            else return anaList[0];
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
            var ctx = new DiversityDataContext();
            return new LightList<AnalysisResult>(from ar in ctx.AnalysisResults
                                                             where ar.AnalysisID == analysisID
                                                             select ar);
        }
        public void addAnalysisResults(IEnumerable<AnalysisResult> results)
        {
            using (var ctx = new DiversityDataContext())
            {
                ctx.AnalysisResults.InsertAllOnSubmit(results);
                ctx.SubmitChanges();
            }
        }

        public IList<AnalysisTaxonomicGroup> getAnalysisTaxonomicGroups(string taxGroup)
        {
            var ctx = new DiversityDataContext();
            return new LightList<AnalysisTaxonomicGroup>(from atg in ctx.AnalysisTaxonomicGroups
                                                 where atg.TaxonomicGroup.Equals(taxGroup)
                                                 select atg);
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
            var ctx = new DiversityDataContext();
            return new LightList<MultimediaObject>(ctx.MultimediaObjects);
        }

        public IList<MultimediaObject> getMultimediaForEventSeries(EventSeries es)
        {
            var ctx = new DiversityDataContext();
            return new LightList<MultimediaObject>(from mmo in ctx.MultimediaObjects
                                                             where mmo.SourceId == (int) SourceID.EventSeries
                                                             && mmo.RelatedId==es.SeriesID
                                                             select mmo);
        }

      
        public IList<MultimediaObject> getMultimediaForEvent(Event ev)
        {

            var ctx = new DiversityDataContext();
            return new LightList<MultimediaObject>(from mmo in ctx.MultimediaObjects
                                                   where mmo.SourceId == (int)SourceID.Event
                                                   && mmo.RelatedId == ev.EventID
                                                   select mmo);
        }


        public IList<MultimediaObject> getMultimediaForSpecimen(Specimen spec)
        {
            var ctx = new DiversityDataContext();
            return new LightList<MultimediaObject>(from mmo in ctx.MultimediaObjects
                                                   where mmo.SourceId == (int)SourceID.Specimen
                                                   && mmo.RelatedId == spec.CollectionSpecimenID
                                                   select mmo);
        }

        public IList<MultimediaObject> getMultimediaForIdentificationUnit(IdentificationUnit iu)
        {
            var ctx = new DiversityDataContext();
            return new LightList<MultimediaObject>(from mmo in ctx.MultimediaObjects
                                                   where mmo.SourceId == (int)SourceID.IdentificationUnit
                                                   && mmo.RelatedId == iu.UnitID
                                                   select mmo);
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
            var ctx = new DiversityDataContext();
            return new LightList<Term>(from t in ctx.Terms
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
                ctx.EventSeries.InsertOnSubmit(new Model.EventSeries() { SeriesID = 0, Description = "ES" });
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

       
    }
}
