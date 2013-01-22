using DiversityORM;
using DiversityService.Model;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService
{
    public partial class DiversityService
    {
        public EventSeries EventSeriesByID(int collectionSeriesID, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                return db.Single<EventSeries>(collectionSeriesID);
            }
        }

        public IEnumerable<Event> EventsByLocality(string locality, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                return db.Query<Event>("[dbo].[DiversityMobile_EventsForProject] (@0, @1) as [Event]", login.ProjectID, locality).Take(15).ToList();
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

                foreach (var iu in ius)
                {
                    iu.Altitude = db.ExecuteScalar<double?>("[dbo].[DiversityMobile_IdentificationUnitAltitude] ( @0 )", iu.CollectionUnitID);
                    iu.Latitude = db.ExecuteScalar<double?>("[dbo].[DiversityMobile_IdentificationUnitLatitude] ( @0 )", iu.CollectionUnitID);
                    iu.Longitude = db.ExecuteScalar<double?>("[dbo].[DiversityMobile_IdentificationUnitLongitude] ( @0 )", iu.CollectionUnitID);
                }

                return ius;
            }
        }
    }
}
