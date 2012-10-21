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
        IEnumerable<GeoPointForSeries> getGeoPointsForSeries(int SeriesID);
        void addOrUpdateGeoPoint(GeoPointForSeries gp);
        void deleteGeoPoint(GeoPointForSeries gp);
        String convertGeoPointsToString(int seriesID);

        IEnumerable<Event> getAllEvents();        
        IEnumerable<Event> getEventsForSeries(EventSeries es);        
        Event getEventByID(int id);
        void addOrUpdateEvent(Event ev);
        void updateEventKey(int oldEventKey, int newEventKey);

        IEnumerable<EventProperty> getPropertiesForEvent(int eventID);
        EventProperty getPropertyByID(int eventId, int propertyId);
        void addOrUpdateCollectionEventProperty(EventProperty cep);

        IEnumerable<Specimen> getAllSpecimen();
        IEnumerable<Specimen> getSpecimenForEvent(Event ev);
        IEnumerable<Specimen> getSpecimenWithoutEvent();
        Specimen getSpecimenByID(int id);
        void addOrUpdateSpecimen(Specimen spec);
        void updateSpecimenKey(int oldSSpecimenKey, int newSpecimenKey);

        IList<IdentificationUnit> getIUForSpecimen(int specimenID);
        IEnumerable<IdentificationUnit> getTopLevelIUForSpecimen(int specimenID);
        IEnumerable<IdentificationUnit> getSubUnits(IdentificationUnit iu);
        IdentificationUnit getIdentificationUnitByID(int id);
        void addOrUpdateIUnit(IdentificationUnit iu);
        void updateIUKey(int oldIUKey, int newIUKey);

        IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu);
        IdentificationUnitAnalysis getIUANByID(int analysisID);
        void addOrUpdateIUA(IdentificationUnitAnalysis iua);
        
        IEnumerable<MultimediaObject> getMultimediaObjectsForUpload();
        IList<MultimediaObject> getMultimediaForObject(IMultimediaOwner owner);        
        MultimediaObject getMultimediaByID(int id);    
        MultimediaObject getMultimediaByURI(string uri);        
        void addMultimediaObject(MultimediaObject mmo);
        void updateMMOUri(string clientUri, string serverUri);
        void updateMMOSuccessfullUpload(string clientUri,string ServerUri,bool success);
        void deleteMMO(MultimediaObject toDeleteMMO);
        
    }
}
