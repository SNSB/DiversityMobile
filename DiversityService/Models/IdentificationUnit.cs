using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class IdentificationUnit
    {
        public int SpecimenID { get; set; }
        public int UnitID { get; set; }
        public int? DiversityCollectionUnitID { get; set; }
        public int? RelatedUnitID { get; set; }


        public bool OnlyObserved { get; set; }
        public string TaxonomicGroup { get; set; }
        public string RelationType { get; set; } //Only on Non-Toplevel
        public string ColonisedSubstratePart { get; set; }
        public string LifeStage { get; set; }
        public string Gender { get; set; }


        //Identification
        public string LastIdentificationCache { get; set; }
        public string FamilyCache { get; set; }
        public string OrderCache { get; set; }
        public string IdentificationUri { get; set; }

        //Georeferenzierung anstelle der IdentificationUnitGeoAnalysis
        public double? Altitude { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime AnalysisDate { get; set; }

        public DateTime LogUpdatedWhen { get; set; }
    }
}
