using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    public class IdentificationUnit
    {
        [Column(IsPrimaryKey=true)]
        public int UnitID { get; set; }

        [Column]
        public int? RelatedUnitID { get; set; }

        [Column]
        public string AccessionNumber { get; set; } //Only on Toplevel
        
        [Column]
        public string TaxonomicGroup { get; set; }

        [Column]
        public string RelationType { get; set; } //Only on Non-Toplevel

        [Column]
        public string ColonisedSubstratePart { get; set; }

        [Column]
        public string LifeStage { get; set; }

        [Column]
        public string Gender { get; set; }

        [Column]
        public string UnitIdentifier { get; set; }

        [Column]
        public string UnitDescription { get; set; }

        [Column]
        public bool? IsModified { get; set; }

    }
}
