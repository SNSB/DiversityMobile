using DiversityORM;
using DiversityPhone.Model;
using DiversityService.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiversityService
{
    public partial class DiversityService : IDiversityService
    {
        public EventSeries EventSeriesByID(int collectionSeriesID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                return db.Single<EventSeries>(collectionSeriesID);
            }
        }

        public IEnumerable<Localization> LocalizationsForSeries(int collectionSeriesID, UserCredentials login)
        {
            //Maybe via functions?
            return Enumerable.Empty<Localization>();
        }

        public IEnumerable<Event> EventsByLocality(string locality, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                return db.Query<Event>("FROM [dbo].[DiversityMobile_EventsForProject] (@0, @1) as [CollectionEvent]", login.ProjectID, locality).Take(15).ToList();
            }
        }

        public IEnumerable<EventProperty> PropertiesForEvent(int collectionEventID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                return db.Query<EventProperty>("WHERE CollectionEventID=@0", collectionEventID).ToList();
            }
        }

        public IEnumerable<Specimen> SpecimenForEvent(int collectionEventID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                return db.Query<Specimen>("WHERE CollectionEventID=@0", collectionEventID).ToList();
            }
        }

        public IEnumerable<IdentificationUnit> UnitsForSpecimen(int collectionSpecimenID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                var ius = db.Query<IdentificationUnit>("WHERE CollectionSpecimenID=@0", collectionSpecimenID).ToList();

                foreach (var iu in ius)
                {
                    AddUnitExternalInformation(iu, db);
                }
                return ius;
            }
        }

        public IEnumerable<IdentificationUnit> SubUnitsForIU(int collectionUnitID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                var ius = db.Query<IdentificationUnit>("WHERE RelatedUnitID=@0", collectionUnitID).ToList();

                foreach (var iu in ius)
                {
                    AddUnitExternalInformation(iu, db);
                }

                return ius;
            }
        }

        private void AddUnitExternalInformation(IdentificationUnit iu, Diversity db)
        {
            var id = db.SingleOrDefault<Identification>("WHERE [CollectionSpecimenID]=@0 AND [IdentificationUnitID]=@1", iu.CollectionSpecimenID, iu.CollectionUnitID);
            if (id != null)
            {
                iu.IdentificationUri = id.NameURI;
                iu.LastIdentificationCache = id.TaxonomicName;
                iu.Qualification = id.IdentificationQualifier;
                iu.AnalysisDate = (id.IdentificationYear.HasValue && id.IdentificationMonth.HasValue && id.IdentificationDay.HasValue)
                    ? new DateTime(id.IdentificationYear.Value, id.IdentificationMonth.Value, id.IdentificationDay.Value, 0, 0, 0)
                    : DateTime.Now;
            }

            iu.Altitude = db.ExecuteScalar<double?>(string.Format("SELECT [dbo].[DiversityMobile_IdentificationUnitAltitude] ( {0} )", iu.CollectionUnitID));
            iu.Latitude = db.ExecuteScalar<double?>(string.Format("SELECT [dbo].[DiversityMobile_IdentificationUnitLatitude] ( {0} )", iu.CollectionUnitID));
            iu.Longitude = db.ExecuteScalar<double?>(string.Format("SELECT [dbo].[DiversityMobile_IdentificationUnitLongitude] ( {0} )", iu.CollectionUnitID));
        }

        public IEnumerable<IdentificationUnitAnalysis> AnalysesForIU(int collectionUnitID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                var analyses = db.Query<IdentificationUnitAnalysis>("WHERE IdentificationUnitID=@0", collectionUnitID).ToList();

                return analyses;
            }
        }
    }
}
