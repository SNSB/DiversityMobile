using PetaPoco;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

/*
 * Contains Model classes that will be sent to the client via WCF
 * 
 */



namespace DiversityService.Model
{
    public class Localization : IEquatable<Localization>
    {

        public double? Altitude { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool Equals(Localization other)
        {
            if (other == null)
                return false;

            if (this.Altitude.HasValue != other.Altitude.HasValue ||
                (this.Altitude.HasValue && !AboutEqual(this.Altitude.Value, other.Altitude.Value)))
                return false;
            if (this.Latitude.HasValue != other.Latitude.HasValue ||
                (this.Latitude.HasValue && !AboutEqual(this.Latitude.Value, other.Latitude.Value)))
                return false;
            if (this.Longitude.HasValue != other.Longitude.HasValue ||
                (this.Longitude.HasValue && !AboutEqual(this.Longitude.Value, other.Longitude.Value)))
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Localization)
                return this.Equals(obj as Localization);
            return false;
        }

        public override int GetHashCode()
        {
            return Altitude.GetHashCode() ^ Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }



        private static bool AboutEqual(double x, double y)
        {
            if (double.IsNaN(x))
                return double.IsNaN(y);

            double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }
    }

    [TableName("CollectionEventSeries")]
    [PrimaryKey("SeriesID", autoIncrement = true)]
    public class EventSeries
    {
        [Column("SeriesID")]
        public int CollectionEventSeriesID { get; set; }

        public string Description { get; set; }

        public string SeriesCode { get; set; }

        [Column("DateStart")]
        public DateTime? SeriesStart { get; set; }

        [Column("DateEnd")]
        public DateTime? SeriesEnd { get; set; }
    }

    [TableName("CollectionEvent")]
    [PrimaryKey("CollectionEventID", autoIncrement = true)]
    public class Event
    {
        public int CollectionEventID { get; set; }

        [Column("SeriesID")]
        public int? CollectionSeriesID { get; set; }

        public DateTime? CollectionDate
        {
            get
            {
                if (CollectionYear.HasValue && CollectionMonth.HasValue && CollectionDay.HasValue)
                    return new DateTime(CollectionYear.Value, CollectionMonth.Value, CollectionDay.Value);
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    CollectionYear = (Int16?)value.Value.Year;
                    CollectionMonth = (byte?)value.Value.Month;
                    CollectionDay = (byte?)value.Value.Day;
                }
                else
                {
                    CollectionYear = null;
                    CollectionMonth = null;
                    CollectionDay = null;
                }

            }
        }
        public string LocalityDescription { get; set; }
        public string HabitatDescription { get; set; }

        //Georeferenzierung anstelle der KLasse CollectionEventLocalisation
        //Altitude hat LocalisationSystem 4
        //WGS84 hat LocalisationSystem 8
        [Ignore]
        public double? Altitude { get; set; }
        [Ignore]
        public double? Latitude { get; set; }
        [Ignore]
        public double? Longitude { get; set; }


        //These 3 Members can't use smaller value sizes because then, 
        //reading from the DB fails when trying to cast
        [IgnoreDataMember]
        public int? CollectionYear { get; set; }
        [IgnoreDataMember]
        public int? CollectionMonth { get; set; }
        [IgnoreDataMember]
        public int? CollectionDay { get; set; }
    }

    [TableName("CollectionEventProperty")]
    public class EventProperty
    {
        public int CollectionEventID { get; set; }
        public int PropertyID { get; set; }
        public String DisplayText { get; set; }
        public String PropertyUri { get; set; }
    }

    [TableName("CollectionSpecimen")]
    [PrimaryKey("CollectionSpecimenID", autoIncrement = true)]
    public class Specimen
    {
        public int CollectionSpecimenID { get; set; }

        [Column("CollectionEventID")]
        public int CollectionEventID { get; set; }

        [Column("DepositorsAccessionNumber")]
        public string AccessionNumber { get; set; }
    }

    public enum MultimediaOwner
    {
        Specimen,
        IdentificationUnit,
        EventSeries,
        Event
    }

    public enum MultimediaType
    {
        Image,
        Video,
        Audio
    }

    public class MultimediaObject
    {
        public MultimediaOwner OwnerType { get; set; }
        public Int32 RelatedCollectionID { get; set; }
        public String Uri { get; set; }
        public String Description { get; set; }
        public MultimediaType MediaType { get; set; }
    }

    [PrimaryKey("IdentificationUnitID", autoIncrement = true)]
    public class IdentificationUnit
    {
        public int CollectionSpecimenID { get; set; }

        [Column("IdentificationUnitID")]
        public int CollectionUnitID { get; set; }

        [Column("RelatedUnitID")]
        public int? CollectionRelatedUnitID { get; set; }

        public bool OnlyObserved { get; set; }
        public string TaxonomicGroup { get; set; }
        public string RelationType { get; set; } //Only on Non-Toplevel
        //public string ColonisedSubstratePart { get; set; }
        //public string LifeStage { get; set; }
        //public string Gender { get; set; }


        //Identification    
        [ResultColumn]
        public string LastIdentificationCache { get; set; }
        /*
        public string FamilyCache { get; set; }
        
        public string OrderCache { get; set; }
        */
        [Ignore]
        public string IdentificationUri { get; set; }
        [Ignore]
        public string Qualification { get; set; }

        //Georeferenzierung anstelle der IdentificationUnitGeoAnalysis
        [Ignore]
        public double? Altitude { get; set; }
        [Ignore]
        public double? Latitude { get; set; }
        [Ignore]
        public double? Longitude { get; set; }
        [Ignore]
        public DateTime AnalysisDate { get; set; }
    }

    public class IdentificationUnitAnalysis
    {
        [Column("IdentificationUnitID")]
        public int? CollectionUnitID { get; set; }
        [Column("CollectionSpecimenID")]
        public int CollectionSpecimenID { get; set; }

        public int AnalysisID { get; set; }

        public int AnalysisNumber { get; set; }

        public string AnalysisResult { get; set; }

        [Ignore]
        public DateTime AnalysisDate { get; set; } //Datum mit Uhrzeit

        //Backing DB Column (varchar(50)) with no reliable formatting... (worse in Test DB)
        [Column("AnalysisDate"), IgnoreDataMember]
        public string CollectionAnalysisDate 
        {
            get
            {
                var tmp = AnalysisDate.ToString("d", new CultureInfo("de-DE"));
                return tmp;
            }
            set
            {
                DateTime tmp;
                if (DateTime.TryParse(value, new CultureInfo("de-DE"), DateTimeStyles.AllowWhiteSpaces, out tmp))
                    AnalysisDate = tmp;
            }
        }
    }
}
