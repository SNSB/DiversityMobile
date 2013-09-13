namespace DiversityPhone.Services
{
    using System.Data.Linq;
    using System.Collections.Generic;
    using System.Data.Linq.Mapping;
    using System;
    using System.Reflection;
    using DiversityPhone.Model;

    public class DiversityDataContext : DataContext
    {
        public static readonly string DB_FILENAME = "diversityDB.sdf";
        public static readonly string DB_URI = "isostore:/diversityDB.sdf";

        public DiversityDataContext()
            : base(DB_URI)
        {
            
        }

        public Table<EventSeries> EventSeries;
        public Table<GeoPointForSeries> GeoTour;

        public Table<Event> Events;
        public Table<EventProperty> EventProperties;
        
        
        public Table<Specimen> Specimen;

        public Table<IdentificationUnit> IdentificationUnits;
        public Table<IdentificationUnitAnalysis> IdentificationUnitAnalyses;        

        public Table<MultimediaObject> MultimediaObjects;        
    }
}
