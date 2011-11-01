namespace DiversityPhone.Services
{
    using System.Collections.Generic;
    using DiversityPhone.Model;

    public interface IOfflineFieldData
    {
        IList<EventSeries> getAllEventSeries();
        EventSeries getEventSeriesByID(int id);
        void addOrUpdateEventSeries(EventSeries newSeries);

        IList<Event> getAllEvents();
        IList<Event> getEventsForSeries(EventSeries es);
        IList<Event> getEventsWithoutSeries();
        void addOrUpdateEvent(Event ev);

        IList<CollectionEventProperty> getPropertiesForEvent(Event ev);
        void addOrUpdateCollectionEventProperty(CollectionEventProperty cep);

        IList<Specimen> getAllSpecimen();
        IList<Specimen> getSpecimenForEvent(Event ev);
        IList<Specimen> getSpecimenWithoutEvent();
        void addOrUpdateSpecimen(Specimen spec);

        IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec);
        IList<IdentificationUnit> getSubUnits(IdentificationUnit iu);
        IdentificationUnit getIUbyID(int id);
        void addOrUpdateIUnit(IdentificationUnit iu);

        IList<IdentificationUnitAnalysis> getIUAForIU(IdentificationUnit iu);
        void addOrUpdateIUA(IdentificationUnitAnalysis iua);

        IList<MultimediaObject> getAllMultimediaObjects();
        IList<MultimediaObject> getMultimediaForEventSeries(EventSeries es);
        IList<MultimediaObject> getMultimediaForEvent(Event ev);
        IList<MultimediaObject> getMultimediaForSpecimen(Specimen spec);
        IList<MultimediaObject> getMultimediaForIdentificationUnit(IdentificationUnit iu);
        void addMultimediaObject(MultimediaObject mmo);


        IList<Map> getAllMaps();
        IList<Map> getMapsForRectangle(double latitudeNorth,double latitudeSouth, double longitudeWest, double longitudeEast);
        void addMap(Map map);
    }
}
