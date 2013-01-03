using System;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using System.Data.Linq;
using System.Text;
using System.Linq.Expressions;
using Svc = DiversityPhone.DiversityService;
using System.IO.IsolatedStorage;
using DiversityPhone.ViewModels;
using Funq;
using System.Reactive;

namespace DiversityPhone.Services
{
   

    public partial class OfflineStorage : IFieldDataService
    {
        private IList<IDisposable> _subscriptions;
        private IMessageBus _messenger;
        private INotificationService _Notifications;
     

        public OfflineStorage(Container ioc)
        {
            this._messenger = ioc.Resolve<IMessageBus>();
            _Notifications = ioc.Resolve<INotificationService>();

            

            _subscriptions = new List<IDisposable>()
            {
                _messenger.Listen<IElementVM<EventSeries>>(MessageContracts.SAVE).Model()                        
                    .Subscribe(es => addOrUpdateEventSeries(es)),
                _messenger.Listen<IElementVM<EventSeries>>(MessageContracts.DELETE).Model()  
                    .Subscribe(es => deleteEventSeries(es)),

                _messenger.Listen<IElementVM<Event>>(MessageContracts.SAVE).Model() 
                    .Subscribe(ev => addOrUpdateEvent(ev)),
                _messenger.Listen<IElementVM<Event>>(MessageContracts.DELETE).Model() 
                    .Subscribe(ev=>deleteEvent(ev)),

                _messenger.Listen<IElementVM<EventProperty>>(MessageContracts.SAVE).Model() 
                    .Subscribe(cep=>addOrUpdateCollectionEventProperty(cep)),
                _messenger.Listen<IElementVM<EventProperty>>(MessageContracts.DELETE).Model() 
                    .Subscribe(cep => deleteEventProperty(cep)),

                _messenger.Listen<IElementVM<Specimen>>(MessageContracts.SAVE).Model() 
                    .Subscribe(spec => addOrUpdateSpecimen(spec)),
                _messenger.Listen<IElementVM<Specimen>>(MessageContracts.DELETE).Model() 
                    .Subscribe(spec=>deleteSpecimen(spec)),

                _messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.SAVE).Model() 
                    .Subscribe(iu => addOrUpdateIUnit(iu)),
                _messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.DELETE).Model() 
                    .Subscribe(iu=>deleteIU(iu)),

                _messenger.Listen<IElementVM<IdentificationUnitAnalysis>>(MessageContracts.SAVE).Model() 
                    .Subscribe(iua=>addOrUpdateIUA(iua)),
                 _messenger.Listen<IElementVM<IdentificationUnitAnalysis>>(MessageContracts.DELETE).Model() 
                    .Subscribe(iua=>deleteIUA(iua)),

                _messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.SAVE).Model()
                    .Subscribe(mmo => addMultimediaObject(mmo)),
                _messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.DELETE).Model()
                    .Subscribe(mmo=>deleteMMO(mmo)),

                _messenger.Listen<GeoPointForSeries>(MessageContracts.SAVE)
                    .Subscribe(gp => addOrUpdateGeoPoint(gp)),
                _messenger.Listen<GeoPointForSeries>(MessageContracts.DELETE)
                    .Subscribe(gp => deleteGeoPoint(gp)),

                _messenger.Listen<ILocalizable>(MessageContracts.SAVE)
                    .Subscribe(loc => 
                        {
                            if (loc is Event)
                                addOrUpdateEvent(loc as Event);
                            else if (loc is IdentificationUnit)
                                addOrUpdateIUnit(loc as IdentificationUnit);
                            else if (loc is GeoPointForSeries)
                                addOrUpdateGeoPoint(loc as GeoPointForSeries);
                        })
                   
            };           
        }

        public void deleteAndNotifyAsync<T>(T detachedRow) where T : class
        {
            _Notifications.showProgress(
                CascadingDelete.deleteCascadingAsync(detachedRow)
                .StartWith(Unit.Default)
                .Select(_ => DiversityResources.Info_DeletingObjects)
                );
        }

       

        #region EventSeries

        private IList<EventSeries> esQuery(Expression<Func<EventSeries, bool>> restriction = null)
        {
            return uncachedQuery(ctx =>
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
            return esQuery(es => es.ModificationState == ModificationState.Modified);
        }

        public EventSeries getEventSeriesByID(int? id)
        {
            if (!id.HasValue)
                return EventSeries.NoEventSeries;

            return singletonQuery(ctx => from es in ctx.EventSeries
                                         where es.SeriesID == id.Value
                                         select es);

        }

        public void addOrUpdateEventSeries(EventSeries newSeries)
        {
            if (EventSeries.isNoEventSeries(newSeries))
                return;
            addOrUpdateRow(EventSeries.Operations, ctx => ctx.EventSeries, newSeries);
            _messenger.SendMessage<EventSeries>(newSeries, MessageContracts.START);
        }

        public void deleteEventSeries(EventSeries toDeleteEs)
        {                   
            
            deleteAndNotifyAsync(toDeleteEs);
        }




        #endregion

        #region GeoPointForSeries

        public IList<GeoPointForSeries> getAllGeoPoints()
        {
            return uncachedQuery(
            ctx =>
                from gt in ctx.GeoTour
                select gt
                );
        }

        public IEnumerable<GeoPointForSeries> getGeoPointsForSeries(int SeriesID)
        {           
            using (var ctx = new DiversityDataContext())
            {
                var query = from gt in ctx.GeoTour
                            where gt.SeriesID == SeriesID
                            select gt;

                foreach (var gp in query)
                    yield return gp;
            }
        }

        public void addOrUpdateGeoPoint(GeoPointForSeries gp)
        {
            addOrUpdateRow(GeoPointForSeries.Operations,
                ctx => ctx.GeoTour,
                gp
            );
        }

        public void deleteGeoPoint(GeoPointForSeries toDeleteGp)
        {
            deleteAndNotifyAsync(toDeleteGp);
        }

        public String convertGeoPointsToString(int seriesID)
        {
            var pointsForSeries = getGeoPointsForSeries(seriesID);
            if (pointsForSeries.Any())
            {
                return string.Format("geography::STGeomFromText('LINESTRING({0})', 4326)",
                        string.Join(", ", pointsForSeries.Select(gp => string.Format("{0} {1}", gp.Longitude, gp.Latitude)))
                    );
            }
            else return String.Empty;
        }


        #endregion

        #region Event

        public IEnumerable<Event> getAllEvents()
        {
            return enumerateQuery(ctx => ctx.Events);
        }


        public IEnumerable<Event> getEventsForSeries(EventSeries es)
        {
            //Workaround for the fact, that ev.SeriesID == es.SeriesID doesn't work for null values
            if (EventSeries.isNoEventSeries(es)) 
                return enumerateQuery(
                    ctx => from ev in ctx.Events
                           where ev.SeriesID == null
                           select ev);

            return enumerateQuery(
                ctx => from ev in ctx.Events
                       where ev.SeriesID == es.SeriesID.Value
                       select ev);
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
                observation.EventID = ev.EventID;
                addOrUpdateSpecimen(observation);

            }
        }

        public void deleteEvent(Event toDeleteEv)
        {
            deleteAndNotifyAsync(toDeleteEv);
        }

        

        #endregion

        #region CollectionEventProperties

        public IEnumerable<EventProperty> getPropertiesForEvent(int eventID)
        {
            return enumerateQuery(ctx =>
                from cep in ctx.EventProperties
                where cep.EventID == eventID 
                select cep
                );
        }

        public EventProperty getPropertyByID(int eventId, int propertyId)
        {
            return singletonQuery(ctx => from cep in ctx.EventProperties
                                         where cep.EventID == eventId &&
                                                cep.PropertyID == propertyId
                                         select cep);
        }

        public void addOrUpdateCollectionEventProperty(EventProperty cep)
        {
            Event ev = this.getEventByID(cep.EventID);
            if (ev.DiversityCollectionEventID != null)
                cep.DiversityCollectionEventID = ev.DiversityCollectionEventID;

            addOrUpdateRow(EventProperty.Operations,
                  ctx => ctx.EventProperties,
                  cep
              );
        }    

        public void deleteEventProperty(EventProperty toDeleteCep)
        {
            deleteAndNotifyAsync(toDeleteCep);
        }

        #endregion


        #region Specimen        
        
        public IEnumerable<Specimen> getAllSpecimen()
        {   
            return enumerateQuery(ctx => ctx.Specimen);
        }


        public IEnumerable<Specimen> getSpecimenForEvent(Event ev)
        {
            return enumerateQuery(ctx =>
                from spec in ctx.Specimen                 
                where spec.EventID == ev.EventID
                select spec
                );
        }
      

        public Specimen getSpecimenByID(int id)
        {
            return singletonQuery(
                ctx => from spec in ctx.Specimen
                       where spec.SpecimenID == id
                       select spec);
        }

        public IEnumerable<Specimen> getSpecimenWithoutEvent()
        {
          return enumerateQuery(ctx =>
                from spec in ctx.Specimen
                where spec.EventID == null 
                select spec
                );
        }

        public void addOrUpdateSpecimen(Specimen spec)
        {
            Event ev = this.getEventByID(spec.EventID);
            if (ev.DiversityCollectionEventID != null)
                spec.DiversityCollectionEventID = ev.DiversityCollectionEventID;
            addOrUpdateRow(Specimen.Operations,
                ctx => ctx.Specimen,
                spec
            );
        }

        public void deleteSpecimen(Specimen toDeleteSpec)
        {
            deleteAndNotifyAsync(toDeleteSpec);
        }



        #endregion

        #region IdentificationUnit

        public IList<IdentificationUnit> getIUForSpecimen(int specimenID)
        {
            return uncachedQuery(
                ctx =>
                    from iu in ctx.IdentificationUnits
                    where iu.SpecimenID == specimenID
                    orderby iu.WorkingName
                    select iu
                    );
        }

        public IEnumerable<IdentificationUnit> getTopLevelIUForSpecimen(int specimenID)
        {
            return enumerateQuery(ctx =>
                from iu in ctx.IdentificationUnits
                where iu.SpecimenID == specimenID && iu.RelatedUnitID == null 
                orderby iu.WorkingName
                select iu
                );
        }


        public IEnumerable<IdentificationUnit> getSubUnits(IdentificationUnit unit)
        {
            return enumerateQuery(ctx =>
                from iu in ctx.IdentificationUnits
                where iu.RelatedUnitID == unit.UnitID
                orderby iu.WorkingName
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
            deleteAndNotifyAsync(toDeleteIU);
        }


        #endregion

        #region Analysis

        public IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu)
        {
            return uncachedQuery(ctx =>
                from iuan in ctx.IdentificationUnitAnalyses
                where iuan.UnitID == iu.UnitID
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
            IdentificationUnit iu = this.getIdentificationUnitByID(iua.UnitID);
            if (iu.DiversityCollectionUnitID != null)
                iua.DiversityCollectionUnitID = iu.DiversityCollectionUnitID;
            addOrUpdateRow(IdentificationUnitAnalysis.Operations,
                ctx => ctx.IdentificationUnitAnalyses,
                iua
            );
        }

        public void deleteIUA(IdentificationUnitAnalysis toDeleteIUA)
        {
            deleteAndNotifyAsync(toDeleteIUA);
        }

        #endregion

        #region Multimedia

        public IList<MultimediaObject> getMultimediaForObject(IMultimediaOwner owner)
        {
             IList<MultimediaObject> objects= uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.OwnerType == owner.OwnerType
                                                && mm.RelatedId == owner.OwnerID
                                        select mm);
             return objects;

        }

        public IEnumerable<MultimediaObject> getMultimediaObjectsForUpload()
        {
            using (var ctx = new DiversityDataContext())
            {
                var q = from mm in ctx.MultimediaObjects
                        where mm.DiversityCollectionRelatedID != null
                        && mm.ModificationState == ModificationState.Modified
                        select mm;
                foreach (var mmo in q)
                {
                    yield return mmo;
                }
            }
        }

        public MultimediaObject getMultimediaByID(int id)
        {
            return singletonQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.MMOID == id
                                        select mm);
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

            addOrUpdateRow(MultimediaObject.Operations,
            ctx => ctx.MultimediaObjects,
            mmo
            ); 
        }

        public void deleteMMO(MultimediaObject toDeleteMMO)
        {
            deleteAndNotifyAsync(toDeleteMMO);            
        }


        #endregion                

        #region Generische Implementierungen
        private void addOrUpdateRow<T>(IQueryOperations<T> operations, Func<DiversityDataContext, Table<T>> tableProvider, T row) where T : class, IModifyable
        {
            if(row == null)
            {
                throw new ArgumentNullException ("row");
            }

            withDataContext((ctx) =>
                {
                    var table = tableProvider(ctx);
                    var allRowsQuery = table as IQueryable<T>;



                    if (row.IsNew())      //New Object
                    {
                        operations.SetFreeKeyOnItem(allRowsQuery, row);
                        row.ModificationState = ModificationState.Modified; //Mark for Upload

                        table.InsertOnSubmit(row);                        
                        try
                        {
                            ctx.SubmitChanges();                            
                        }
                        catch (Exception)
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
                                    catch (Exception)
                                    {
                                        System.Diagnostics.Debugger.Break();
                                    }
                                });
                        }
                    }              
                });
        }

        private T singletonQuery<T>(Func<DiversityDataContext, IQueryable<T>> queryProvider)
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

        private IList<T> uncachedQuery<T>(Func<DiversityDataContext, IQueryable<T>> query)
        {
            IList<T> result = null;
            withDataContext(ctx => result = query(ctx).ToList());
            return result;
        }


        private IEnumerable<T> enumerateQuery<T>(Func<DiversityDataContext, IQueryable<T>> query)
        {
            using (var ctx = new DiversityDataContext())
            {
                var q = query(ctx);

                foreach (var res in q)
                {
                    yield return res;
                }
            }
        }       

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

                IQueryable<EventProperty> clientPropertyList =
                    from cep in ctx.EventProperties
                    where cep.EventID == ev.EventID && cep.ModificationState == ModificationState.Modified
                    select cep;
                foreach (EventProperty cep in clientPropertyList)
                {
                    Svc.CollectionEventProperty serverCep = EventProperty.ConvertToServiceObject(cep);
                    result.Properties.Add(serverCep);
                }

                IQueryable<Specimen> clientSpecList =
                    from spec in ctx.Specimen
                    where spec.EventID == ev.EventID
                    select spec;
                foreach (Specimen spec in clientSpecList)
                {
                    if (spec.ModificationState == ModificationState.Modified)
                    {
                        Svc.Specimen serverSpec = Specimen.ConvertToServiceObject(spec);
                        result.Specimen.Add(serverSpec);
                    }
                    IQueryable<IdentificationUnit> clientIUListForSpec =
                        from iu in ctx.IdentificationUnits
                        where iu.SpecimenID== spec.SpecimenID
                        select iu;
                    foreach (IdentificationUnit iu in clientIUListForSpec)
                    {
                        if (iu.ModificationState == ModificationState.Modified)
                        {
                            Svc.IdentificationUnit serverIU = IdentificationUnit.ConvertToServiceObject(iu);
                            result.IdentificationUnits.Add(serverIU);
                        }

                        IQueryable<IdentificationUnitAnalysis> clientIUAListForIU =
                            from iua in ctx.IdentificationUnitAnalyses
                            where iua.UnitID == iu.UnitID && iu.ModificationState == ModificationState.Modified
                            select iua;
                        foreach (IdentificationUnitAnalysis iua in clientIUAListForIU)
                        {
                            Svc.IdentificationUnitAnalysis serverIUA = IdentificationUnitAnalysis.ConvertToServiceObject(iua,iu);
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
                           where es.ModificationState == ModificationState.Modified
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
                clientSeries.ModificationState = ModificationState.Unmodified;
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
            using (var ctx = new DiversityDataContext())
            {
                var savedEvents =
                    from ev in ctx.Events
                    where ev.EventID == clientKey
                    select ev;
                Event clientEvent = savedEvents.First();//TODO: Check if there is a key valuation
                clientEvent.DiversityCollectionEventID = serverKey;
                clientEvent.ModificationState = ModificationState.Unmodified;
                ctx.SubmitChanges();
                var savedSpecimen =
                    from spec in ctx.Specimen
                    where spec.EventID == clientKey
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
                    from cep in ctx.EventProperties
                    where cep.EventID == clientKey
                    select cep;
                foreach (EventProperty cep in ceProperties)
                {
                    cep.DiversityCollectionEventID = serverKey;
                    cep.ModificationState = ModificationState.Unmodified;
                }
                ctx.SubmitChanges();
            }
        }

        public void updateSpecimenKey(int clientKey, int serverKey)
        {
            using (DiversityDataContext ctx = new DiversityDataContext())
            {
                var savedSpecimens =
                    from spec in ctx.Specimen
                    where spec.SpecimenID == clientKey
                    select spec;
                Specimen clientSpecimen = savedSpecimens.First();//TODO: Check if there is a key valuation
                clientSpecimen.DiversityCollectionSpecimenID = serverKey;
                clientSpecimen.ModificationState = ModificationState.Unmodified;
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
            using (var ctx = new DiversityDataContext())
            {
                var savedIUs =
                    from iu in ctx.IdentificationUnits
                    where iu.UnitID == clientKey
                    select iu;
                IdentificationUnit clientIU = savedIUs.First();//TODO: Check if there is a key valuation
                clientIU.DiversityCollectionUnitID= serverKey;
                clientIU.ModificationState = ModificationState.Unmodified;
                var relatedIU =
                    from iu in ctx.IdentificationUnits
                    where iu.RelatedUnitID == clientKey
                    select iu;
                foreach (IdentificationUnit iu in relatedIU)
                    iu.DiversityCollectionRelatedUnitID = serverKey;
                var iuaList =
                    from iua in ctx.IdentificationUnitAnalyses
                    where iua.UnitID == clientKey
                    select iua;
                foreach (IdentificationUnitAnalysis iua in iuaList)
                {
                    iua.DiversityCollectionUnitID = serverKey;
                    iua.ModificationState = ModificationState.Unmodified;
                }
                var iuMMO =
                    from mmo in ctx.MultimediaObjects
                    where mmo.RelatedId == clientKey && mmo.OwnerType == ReferrerType.IdentificationUnit
                    select mmo;
                foreach (MultimediaObject mmo in iuMMO)
                    mmo.DiversityCollectionRelatedID = serverKey;
                ctx.SubmitChanges();
            }
        }

        public void updateMMOUri(string clientUri, string serverUri)
        {
            using (DiversityDataContext ctx = new DiversityDataContext())
            {
                try
                {
                   var mmo =
                        from mmos in ctx.MultimediaObjects
                        where mmos.Uri == clientUri
                        select mmos;
                   if (mmo.Count() != 1)
                       throw new Exception("Uri not unique or not present");
                   mmo.First().DiversityCollectionUri = serverUri;
                   ctx.SubmitChanges();
                }
                catch (Exception)
                {
                    throw new Exception("Unable to update uri");
                }
            }
        }

        public void updateMMOSuccessfullUpload(string clientUri,string serverUri, bool success)
        {
            using (DiversityDataContext ctx = new DiversityDataContext())
            {
                try
                {
                    var mmo =
                         from mmos in ctx.MultimediaObjects
                         where mmos.Uri == clientUri
                         select mmos;
                    if (mmo.Count() != 1)
                        throw new Exception("Uri not unique or not present");
                    if (success && mmo.First().DiversityCollectionUri.Equals(serverUri))
                    {
                        mmo.First().ModificationState = ModificationState.Unmodified;
                        ctx.SubmitChanges();
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Unable to update State");
                }
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
