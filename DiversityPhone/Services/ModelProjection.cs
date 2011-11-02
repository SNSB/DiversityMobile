using System;
using Client = DiversityPhone.Model;
using Service = DiversityPhone.Service;

namespace DiversityPhone.Services
{
    public static class ModelProjection
    {
        public static Service.EventSeries ToModel(this Client.EventSeries es)
        {
            return new Service.EventSeries()
            {
                Description = es.Description,
                SeriesCode = es.SeriesCode,
                SeriesStart = es.SeriesStart,
                SeriesEnd = es.SeriesEnd,
                SeriesID = es.SeriesID,
                //TODO Geography
            };            
        }

        public static Service.Event ToModel(this Client.Event ev)
        {
            return new Service.Event()
            {
                EventID = ev.EventID,
                SeriesID = ev.SeriesID,
                CollectionDate = ev.CollectionDate,
                DeterminationDate = ev.DeterminationDate,
                HabitatDescription = ev.HabitatDescription,
                LocalityDescription = ev.LocalityDescription,
                LogUpdatedWhen = ev.LogUpdatedWhen,                

                Altitude = ev.Altitude,
                Latitude = ev.Latitude,
                Longitude = ev.Longitude,                
            };
        }
    }
}
