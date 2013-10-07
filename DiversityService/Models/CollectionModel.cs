using PetaPoco;
using System;

namespace DiversityService.Model
{
    public class IdentificationUnitGeoAnalysis
    {
        public DateTime AnalysisDate { get; set; }
        public int IdentificationUnitID { get; set; }
        public int CollectionSpecimenID { get; set; }
        public string ResponsibleName { get; set; }
        public string ResponsibleAgentURI { get; set; }
    }
    public class Identification
    {
        public int CollectionSpecimenID { get; set; }
        public int IdentificationUnitID { get; set; }
        public int IdentificationSequence { get; set; }
        public byte? IdentificationDay { get; set; }
        public byte? IdentificationMonth { get; set; }
        public short? IdentificationYear { get; set; }
        public string IdentificationDateCategory { get; set; }
        public string TaxonomicName { get; set; }
        public string NameURI { get; set; }
        public string IdentificationCategory { get; set; }
        public string ResponsibleName { get; set; }
        public string ResponsibleAgentURI { get; set; }
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
        public string Location1 { get; set; }
        public string Location2 { get; set; }
        public DateTime? DeterminationDate { get; set; }
        public string ResponsibleName { get; set; }
        public string ResponsibleAgentURI { get; set; }
        public string RecordingMethod { get; set; }

        [Ignore]
        public string Geography { get; set; }
        public double? AverageAltitudeCache { get; set; }
        public double? AverageLatitudeCache { get; set; }
        public double? AverageLongitudeCache { get; set; }

    }

    public class CollectionAgent
    {
        public int CollectionSpecimenID { get; set; }
        public string CollectorsAgentURI { get; set; }
        public string CollectorsName { get; set; }
    }

   
}
