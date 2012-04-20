using System;
using PetaPoco;


namespace DiversityService.Model
{
    public class IdentificationUnitAnalysis
    {
        [Ignore]
        public int IdentificationUnitID { get; set; }
        [Column("IdentificationUnitID")]
        public int? DiversityCollectionUnitID { get; set; }
        [Ignore]
        public int SpecimenID{get;set;}
        [Column("CollectionSpecimenID")]
        public int DiversityCollectionSpecimenID { get; set; }

        public int AnalysisID { get; set; }

        [Column("AnalysisNumber")]
        public int IdentificationUnitAnalysisID { get; set; }
        public string AnalysisResult { get; set; }
        public DateTime AnalysisDate { get; set; } //Datum mit Uhrzeit
    }
}
