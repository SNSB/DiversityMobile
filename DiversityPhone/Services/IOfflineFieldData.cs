using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public interface IOfflineFieldData
    {
        IQueryable<EventSeries> EventSeries { get; }
        IList<EventSeries> getAllEventSeries();
        IList<EventSeries> getEventSeriesByDescription(string query);
        IList<EventSeries> getNewEventSeries();
        EventSeries getEventSeriesByID(int id);
        void addEventSeries(EventSeries newSeries);

        IList<Event> getAllEvents();
        IList<Event> getEventsForSeries(EventSeries es);
        void addEvent(Event ev);

        IList<IdentificationUnit> getIUForEvent(Event ev);

        //IList<Event> Events { get; }
        //IList<IdentificationUnit> IdentificationUnits { get; }
    }
}
