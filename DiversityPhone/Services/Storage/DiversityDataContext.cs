using DiversityPhone.Model;
using System.Data.Linq;

namespace DiversityPhone.Services {


    public class DiversityDataContext : DataContext {
        public const string DB_FILENAME = "DiversityDB.sdf";

        private static string GetCurrentProfileDBPath() {
            var profilePath = App.Profile.CurrentProfilePath();
            return string.Format("{0}/{1}", profilePath.Trim('/'), DB_FILENAME);
        }

        public DiversityDataContext()
            : this(GetCurrentProfileDBPath()) {

        }

        public DiversityDataContext(
            string DatabaseFilePath
            )
            : base(string.Format("isostore:/{0}", DatabaseFilePath.TrimStart('/'))) {
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
