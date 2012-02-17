namespace DiversityPhone.Services
{
using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;

    public interface IOfflineFieldData
    {
        void clearDatabase();


        Svc.HierarchySection getNewHierarchyToSyncBelow(Event ev, Svc.UserProfile profile, int projectID);
        IList<Svc.EventSeries> getUploadServiceEventSeries();
        void updateHierarchy(Svc.HierarchySection from, Svc.HierarchySection to);

        IList<EventSeries> getAllEventSeries();
        EventSeries getEventSeriesByID(int id);
        IList<EventSeries> getNewEventSeries();
        void addOrUpdateEventSeries(EventSeries newSeries);
        void deleteEventSeries(EventSeries es);
        void updateSeriesKey(int oldSeriesKey, int newSeriesKey);

        IList<Event> getAllEvents();        
        IList<Event> getEventsForSeries(EventSeries es);        
        Event getEventByID(int id);
        void addOrUpdateEvent(Event ev);
        void updateEventKey(int oldEventKey, int newEventKey);

        IList<CollectionEventProperty> getPropertiesForEvent(Event ev);
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
        MultimediaObject getMultimediaByURI(string uri);        
        void addMultimediaObject(MultimediaObject mmo);


        IList<Map> getAllMaps();
        IList<Map> getMapsForRectangle(double latitudeNorth,double latitudeSouth, double longitudeWest, double longitudeEast);
        void addMap(Map map);
    }
}
