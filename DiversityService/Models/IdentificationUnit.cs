using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace DiversityService.Model
{

    [PrimaryKey("IdentificationUnitID", autoIncrement = true)]
    public class IdentificationUnit
    {
        [Ignore]
        public int SpecimenID { get; set; }
        [Column("CollectrionSpecimenID")]
        public int? DiversityCollectionSpecimenID { get; set; }
        [Ignore]
        public int UnitID { get; set; }
        [Column("IdentificationUnitID")]
        public int DiversityCollectionUnitID { get; set; }
        [Ignore]
        public int? RelatedUnitID { get; set; }
        [Column("RelatedUnitID")]
        public int? DiversityCollectionRelatedUnitID { get; set; }

        public bool OnlyObserved { get; set; }
        public string TaxonomicGroup { get; set; }
        public string RelationType { get; set; } //Only on Non-Toplevel
        public string ColonisedSubstratePart { get; set; }
        public string LifeStage { get; set; }
        public string Gender { get; set; }


        //Identification
        [Ignore]
        public string LastIdentificationCache { get; set; }
        [Ignore]
        public string FamilyCache { get; set; }
        [Ignore]
        public string OrderCache { get; set; }
        [Ignore]
        public string IdentificationUri { get; set; }

        //Georeferenzierung anstelle der IdentificationUnitGeoAnalysis
        [Ignore]
        public double? Altitude { get; set; }
        [Ignore]
        public double? Latitude { get; set; }
        [Ignore]
        public double? Longitude { get; set; }
        [Ignore]
        public DateTime AnalysisDate { get; set; }
        [Ignore]
        public DateTime LogUpdatedWhen { get; set; }
    }
}
