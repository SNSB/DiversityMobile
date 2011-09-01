using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class IdentificationUnit
    {
        public int UnitID { get; set; }
        public int? RelatedUnitID { get; set; }

        public string TaxonomicGroup { get; set; }
        public string RelationType { get; set; }
        public string ColonisedSubstratePart { get; set; }
        public string LifeStage { get; set; }
        public string Gender { get; set; }
        public string UnitIdentifier { get; set; }
        public string UnitDescription { get; set; }

        public bool? IsModified { get; set; }

    }
}
