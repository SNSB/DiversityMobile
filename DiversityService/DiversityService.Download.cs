using DiversityORM;
using DiversityService.Model;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService
{
    public partial class DiversityService : IDiversityService
    {
        public EventSeries EventSeriesByID(int collectionSeriesID, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                return db.Single<EventSeries>(collectionSeriesID);
            }
        }

        public IEnumerable<Localization> LocalizationsForSeries(int collectionSeriesID, UserCredentials login)
        {
            //Maybe via functions?
            throw new NotImplementedException();
        }

        public IEnumerable<Event> EventsByLocality(string locality, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                return db.Query<Event>("[dbo].[DiversityMobile_EventsForProject] (@0, @1) as [Event]", login.ProjectID, locality).Take(15).ToList();
            }
        }

        public IEnumerable<EventProperty> PropertiesForEvent(int collectionEventID, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                return db.Query<EventProperty>("WHERE CollectionEventID=@0", collectionEventID).ToList();
            }
        }

        public IEnumerable<Specimen> SpecimenForEvent(int collectionEventID, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                return db.Query<Specimen>("WHERE CollectionEventID=@0", collectionEventID).ToList();
            }
        }

        public IEnumerable<IdentificationUnit> UnitsForSpecimen(int collectionSpecimenID, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                var ius = db.Query<IdentificationUnit>("WHERE CollectionSpecimenID=@0", collectionSpecimenID).ToList();

                AddUnitGeoInformation(ius, db);

                return ius;
            }
        }

        public IEnumerable<IdentificationUnit> SubUnitsForIU(int collectionUnitID, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                var ius = db.Query<IdentificationUnit>("WHERE RelatedUnitID=@0", collectionUnitID).ToList();

                AddUnitGeoInformation(ius, db);

                return ius;
            }
        }

        private void AddUnitGeoInformation(IEnumerable<IdentificationUnit> units, Diversity db)
        {
            foreach (var iu in units)
            {
                iu.Altitude = db.ExecuteScalar<double?>("[dbo].[DiversityMobile_IdentificationUnitAltitude] ( @0 )", iu.CollectionUnitID);
                iu.Latitude = db.ExecuteScalar<double?>("[dbo].[DiversityMobile_IdentificationUnitLatitude] ( @0 )", iu.CollectionUnitID);
                iu.Longitude = db.ExecuteScalar<double?>("[dbo].[DiversityMobile_IdentificationUnitLongitude] ( @0 )", iu.CollectionUnitID);
            }
        }

        public IEnumerable<IdentificationUnitAnalysis> AnalysesForIU(int collectionUnitID, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                var analyses = db.Query<IdentificationUnitAnalysis>("WHERE IdentificationUnitID=@0", collectionUnitID).ToList();

                return analyses;
            }
        }
    }
}
