namespace DiversityPhone.Services
{
using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.Service;

    public interface IOfflineFieldData
    {
        Svc.HierarchySection getNewHierarchyBelow(Event ev);
        void updateHierarchy(Svc.HierarchySection from, Svc.HierarchySection to);

        IList<EventSeries> getAllEventSeries();
        EventSeries getEventSeriesByID(int id);
        IList<EventSeries> getNewEventSeries();
        void addOrUpdateEventSeries(EventSeries newSeries);
        void deleteEventSeries(EventSeries es);

        IList<Event> getAllEvents();        
        IList<Event> getEventsForSeries(EventSeries es);        
        Event getEventByID(int id);
        void addOrUpdateEvent(Event ev);

        IList<CollectionEventProperty> getPropertiesForEvent(Event ev);
        void addOrUpdateCollectionEventProperty(CollectionEventProperty cep);

        IList<Specimen> getAllSpecimen();
        IList<Specimen> getSpecimenForEvent(Event ev);
        IList<Specimen> getSpecimenWithoutEvent();
        Specimen getSpecimenByID(int id);
        void addOrUpdateSpecimen(Specimen spec);

        IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec);
        IList<IdentificationUnit> getSubUnits(IdentificationUnit iu);
        IdentificationUnit getIdentificationUnitByID(int id);
        void addOrUpdateIUnit(IdentificationUnit iu);

        IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu);
        IdentificationUnitAnalysis getIUANByID(int analysisID);
        void addOrUpdateIUA(IdentificationUnitAnalysis iua);

        IList<MultimediaObject> getAllMultimediaObjects();
        MultimediaObject getMultimediaForObject(ReferrerType objectType, int id);
        MultimediaObject getMultimediaByURI(string uri);        
        void addMultimediaObject(MultimediaObject mmo);


        IList<Map> getAllMaps();
        IList<Map> getMapsForRectangle(double latitudeNorth,double latitudeSouth, double longitudeWest, double longitudeEast);
        void addMap(Map map);
    }
}
