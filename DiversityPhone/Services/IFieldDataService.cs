namespace DiversityPhone.Services
{
using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;
using System;

    public interface IFieldDataService
    {
        void clearDatabase();


        Svc.HierarchySection getNewHierarchyToSyncBelow(Event ev);
        IList<EventSeries> getUploadServiceEventSeries();
        void updateHierarchy(Svc.HierarchySection from, Svc.HierarchySection to);

        IList<EventSeries> getAllEventSeries();
        EventSeries getEventSeriesByID(int id);
        IList<EventSeries> getNewEventSeries();
        void addOrUpdateEventSeries(EventSeries newSeries);
        void deleteEventSeries(EventSeries es);
        void updateSeriesKey(int oldSeriesKey, int newSeriesKey);

        IList<GeoPointForSeries> getAllGeoPoints();
        IList<GeoPointForSeries> getGeoPointsForSeries(int SeriesID);
        void addOrUpdateGeopPoint(GeoPointForSeries gp);
        void deleteGeoPoint(GeoPointForSeries gp);
        String convertGeoPointsToString(int seriesID);

        IList<Event> getAllEvents();        
        IList<Event> getEventsForSeries(EventSeries es);        
        Event getEventByID(int id);
        void addOrUpdateEvent(Event ev);
        void updateEventKey(int oldEventKey, int newEventKey);

        IList<CollectionEventProperty> getPropertiesForEvent(int eventID);
        CollectionEventProperty getPropertyByID(int eventId, int propertyId);
        void addOrUpdateCollectionEventProperty(CollectionEventProperty cep);

        IList<Specimen> getAllSpecimen();
        IList<Specimen> getSpecimenForEvent(Event ev);
        IList<Specimen> getSpecimenWithoutEvent();
        Specimen getSpecimenByID(int id);
        void addOrUpdateSpecimen(Specimen spec);
        void updateSpecimenKey(int oldSSpecimenKey, int newSpecimenKey);

        IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec);
        IList<IdentificationUnit> getSubUnits(IdentificationUnit iu);
        IdentificationUnit getIdentificationUnitByID(int id);
        void addOrUpdateIUnit(IdentificationUnit iu);
        void updateIUKey(int oldIUKey, int newIUKey);

        IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu);
        IdentificationUnitAnalysis getIUANByID(int analysisID);
        void addOrUpdateIUA(IdentificationUnitAnalysis iua);

        IList<MultimediaObject> getAllMultimediaObjects();
        IList<MultimediaObject> getMultimediaForObject(ReferrerType objectType, int id);
        IList<MultimediaObject> getMultimediaForObjectAndType(ReferrerType objectType, int id,MediaType type);
        MultimediaObject getMultimediaByURI(string uri);        
        void addMultimediaObject(MultimediaObject mmo);
        
    }
}
