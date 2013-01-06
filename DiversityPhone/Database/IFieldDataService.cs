namespace DiversityPhone.Services
{
using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;
using System;

    public interface IFieldDataService
    {
        void clearDatabase();

        IList<EventSeries> getAllEventSeries();
        EventSeries getEventSeriesByID(int? id);        
        void addOrUpdateEventSeries(EventSeries newSeries);
        void deleteEventSeries(EventSeries es);        
                
        IEnumerable<GeoPointForSeries> getGeoPointsForSeries(int SeriesID);
        void addGeoPoint(GeoPointForSeries gp);
        void deleteGeoPoint(GeoPointForSeries gp);        

        IEnumerable<Event> getAllEvents();        
        IEnumerable<Event> getEventsForSeries(EventSeries es);        
        Event getEventByID(int id);
        void addOrUpdateEvent(Event ev);        

        IEnumerable<EventProperty> getPropertiesForEvent(int eventID);
        EventProperty getPropertyByID(int eventId, int propertyId);
        void addOrUpdateCollectionEventProperty(EventProperty cep);

        IEnumerable<Specimen> getAllSpecimen();
        IEnumerable<Specimen> getSpecimenForEvent(Event ev);
        IEnumerable<Specimen> getSpecimenWithoutEvent();
        Specimen getSpecimenByID(int id);
        void addOrUpdateSpecimen(Specimen spec);       

        IList<IdentificationUnit> getIUForSpecimen(int specimenID);
        IEnumerable<IdentificationUnit> getTopLevelIUForSpecimen(int specimenID);
        IEnumerable<IdentificationUnit> getSubUnits(IdentificationUnit iu);
        IdentificationUnit getIdentificationUnitByID(int id);
        void addOrUpdateIUnit(IdentificationUnit iu);        

        IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu);
        IdentificationUnitAnalysis getIUANByID(int analysisID);
        void addOrUpdateIUA(IdentificationUnitAnalysis iua);        
        
        IList<MultimediaObject> getMultimediaForObject(IMultimediaOwner owner);        
        MultimediaObject getMultimediaByID(int id);    
        MultimediaObject getMultimediaByURI(string uri);
        IEnumerable<MultimediaObject> getModifiedMMOs();
        void addMultimediaObject(MultimediaObject mmo);
        void deleteMMO(MultimediaObject toDeleteMMO);
    }
}
