using PetaPoco;
using System;

namespace DiversityService.Model
{
    public class IdentificationUnitGeoAnalysis
    {
        public DateTime AnalysisDate { get; set; }
        public int IdentificationUnitID { get; set; }
        public int CollectionSpecimenID { get; set; }
        public String ResponsibleName { get; set; }
        public String ResponsibleAgentURI { get; set; }
    }
    public class Identification
    {
        public int CollectionSpecimenID { get; set; }
        public int IdentificationUnitID { get; set; }
        public int IdentificationSequence { get; set; }
        public byte? IdentificationDay { get; set; }
        public byte? IdentificationMonth { get; set; }
        public short? IdentificationYear { get; set; }
        public String IdentificationDateCategory { get; set; }
        public String TaxonomicName { get; set; }
        public String NameURI { get; set; }
        public String IdentificationCategory { get; set; }
        public String ResponsibleName { get; set; }
        public String ResponsibleAgentURI { get; set; }
        public string IdentificationQualifier { get; set; }
    }

    public class CollectionProject
    {
        public int CollectionSpecimenID { get; set; }
        public int ProjectID { get; set; }
    }

    public class CollectionEventLocalisation
    {
        public int CollectionEventID { get; set; }
        public int LocalisationSystemID { get; set; }
        public String Location1 { get; set; }
        public String Location2 { get; set; }
        public DateTime? DeterminationDate { get; set; }
        public String ResponsibleName { get; set; }
        public String ResponsibleAgentURI { get; set; }
        public String RecordingMethod { get; set; }

        [Ignore]
        public String Geography { get; set; }
        public double? AverageAltitudeCache { get; set; }
        public double? AverageLatitudeCache { get; set; }
        public double? AverageLongitudeCache { get; set; }

    }

    public class CollectionAgent
    {
        public int CollectionSpecimenID { get; set; }
        public String CollectorsAgentURI { get; set; }
        public String CollectorsName { get; set; }
    }

   
}
