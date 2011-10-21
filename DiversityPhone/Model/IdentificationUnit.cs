

namespace DiversityPhone.Model
{
    using System;
    using System.Data.Linq.Mapping;

    [Table]
    public class IdentificationUnit
    {
        public IdentificationUnit()
        {
            this.IsModified = null;
            this.RelatedUnitID = null;
        }

        [Column(IsPrimaryKey = true)]
        public int SpecimenID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int UnitID { get; set; }

        [Column]
        public int? RelatedUnitID { get; set; }


        [Column]
        public bool OnlyObserved { get; set; }
        
        [Column]
        public string TaxonomicGroup { get; set; }

        [Column]
        public string RelationType { get; set; } // Only on Non-Toplevel

        [Column]
        public string ColonisedSubstratePart { get; set; }

        [Column]
        public string LifeStage { get; set; }

        [Column]
        public string Gender { get; set; }

     

        //Identification
        [Column]
        public string LastIdentificationCache { get; set; }
        [Column]
        public string FamilyCache { get; set; }
        [Column]
        public string OrderCache { get; set; }
        [Column]
        public string IdentificationUri { get; set; }


        //Georeferenzierung
        [Column]
        public double Altitude { get; set; }

        [Column]
        public double Latitude { get; set; }

        [Column]
        public double Longitude { get; set; }

        [Column]
        public DateTime AnalysisDate { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }
        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }

    }
}
