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
        EventSeries getEventSeriesByID(int? id);
        IList<EventSeries> getNewEventSeries();
        void addOrUpdateEventSeries(EventSeries newSeries);
        void deleteEventSeries(EventSeries es);
        void updateSeriesKey(int clientSeriesKey, int serverSeriesKey);

        IList<GeoPointForSeries> getAllGeoPoints();
        IList<GeoPointForSeries> getGeoPointsForSeries(int SeriesID);
        void addOrUpdateGeoPoint(GeoPointForSeries gp);
        void deleteGeoPoint(GeoPointForSeries gp);
        String convertGeoPointsToString(int seriesID);

        IList<Event> getAllEvents();        
        IList<Event> getEventsForSeries(EventSeries es);        
        Event getEventByID(int id);
        void addOrUpdateEvent(Event ev);
        void updateEventKey(int oldEventKey, int newEventKey);

        IList<EventProperty> getPropertiesForEvent(int eventID);
        EventProperty getPropertyByID(int eventId, int propertyId);
        void addOrUpdateCollectionEventProperty(EventProperty cep);

        IList<Specimen> getAllSpecimen();
        IList<Specimen> getSpecimenForEvent(Event ev);
        IList<Specimen> getSpecimenWithoutEvent();
        Specimen getSpecimenByID(int id);
        void addOrUpdateSpecimen(Specimen spec);
        void updateSpecimenKey(int oldSSpecimenKey, int newSpecimenKey);

        IList<IdentificationUnit> getIUForSpecimen(int specimenID);
        IList<IdentificationUnit> getTopLevelIUForSpecimen(int specimenID);
        IList<IdentificationUnit> getSubUnits(IdentificationUnit iu);
        IdentificationUnit getIdentificationUnitByID(int id);
        void addOrUpdateIUnit(IdentificationUnit iu);
        void updateIUKey(int oldIUKey, int newIUKey);

        IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu);
        IdentificationUnitAnalysis getIUANByID(int analysisID);
        void addOrUpdateIUA(IdentificationUnitAnalysis iua);

        IList<MultimediaObject> getAllMultimediaObjects();
        IList<MultimediaObject> getMultimediaObjectsForUpload();
        IList<MultimediaObject> getMultimediaForObject(ReferrerType objectType, int id);
        IList<MultimediaObject> getMultimediaForObjectAndType(ReferrerType objectType, int id,MediaType type);
        MultimediaObject getMultimediaByID(int id);    
        MultimediaObject getMultimediaByURI(string uri);        
        void addMultimediaObject(MultimediaObject mmo);
        void updateMMOUri(string clientUri, string serverUri);
        void updateMMOSuccessfullUpload(string clientUri,string ServerUri,bool success);
        void deleteMMO(MultimediaObject toDeleteMMO);
        
    }
}
