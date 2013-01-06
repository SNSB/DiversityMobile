using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * Contains Model classes that will be sent to the client via WCF
 * 
 */



namespace DiversityService.Model
{
    public class Localization
    {
        public double? Altitude { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }        
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

        public DateTime CollectionDate { get; set; }
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
        [Ignore]
        public DateTime? DeterminationDate { get; set; } 
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
        public string ColonisedSubstratePart { get; set; }
        public string LifeStage { get; set; }
        public string Gender { get; set; }


        //Identification

        public string LastIdentificationCache { get; set; }
        [Ignore]
        public string FamilyCache { get; set; }
        [Ignore]
        public string OrderCache { get; set; }
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
        public DateTime AnalysisDate { get; set; } //Datum mit Uhrzeit
    }
}
