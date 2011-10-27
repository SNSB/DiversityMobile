namespace DiversityPhone.Services
{
using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.Service;

    public interface IOfflineFieldData
    {
        Svc.HierarchySection getNewHierarchyBelow(EventSeries es);

        IList<EventSeries> getAllEventSeries();
        IList<EventSeries> getEventSeriesByDescription(string query);
        IList<EventSeries> getNewEventSeries();
        EventSeries getEventSeriesByID(int id);
        void addOrUpdateEventSeries(EventSeries newSeries);

        IList<Event> getAllEvents();
        IList<Event> getEventsForSeries(EventSeries es);
        void addOrUpdateEvent(Event ev);

        IList<Specimen> getAllSpecimen();
        IList<Specimen> getSpecimenForEvent(Event ev);
        void addOrUpdateSpecimen(Specimen spec);

        IList<IdentificationUnit> getTopLevelIUForSpecimen(Specimen spec);
        IList<IdentificationUnit> getSubUnits(IdentificationUnit iu);
        void addOrUpdateIUnit(IdentificationUnit iu);
    }
}
