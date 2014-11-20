namespace DiversityPhone.Interface
{
    using DiversityPhone.Model;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    public interface IFieldDataService
    {
        void ClearDatabase();

        IList<EventSeries> getAllEventSeries();

        EventSeries getEventSeriesByID(int? id);

        void addOrUpdateEventSeries(EventSeries newSeries);

        void deleteEventSeries(EventSeries es);

        IEnumerable<Localization> getGeoPointsForSeries(int SeriesID);

        void addGeoPoint(Localization gp);

        void deleteGeoPoint(Localization gp);

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

        void add<T>(T element) where T : class, IModifyable;

        void addAll<T>(IEnumerable<T> elements) where T : class, IModifyable;

        void update<T>(T element, Action<T> updateValues) where T : class;

        void delete<T>(T element) where T : class;

        T get<T>(int? id) where T : class, IEntity;
    }

    public static class FieldDataMixin
    {
        public static T MarkUploaded<T>(this IFieldDataService This, T entity) where T : class, IModifyable
        {
            Contract.Requires(This != null);
            Contract.Requires(entity != null);
            Contract.Requires(entity.ModificationState == ModificationState.Modified);
            //Contract.Ensures(entity.ModificationState == ModificationState.Unmodified);

            This.update(entity, e => e.ModificationState = ModificationState.Unmodified);

            return entity;
        }
    }
}