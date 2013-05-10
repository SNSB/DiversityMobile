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
        private static string isolatedStorageDBUri = "isostore:/diversityDB.sdf";

        public DiversityDataContext()
            : base(isolatedStorageDBUri)
        {
            if (!this.DatabaseExists())
                this.CreateDatabase();
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
