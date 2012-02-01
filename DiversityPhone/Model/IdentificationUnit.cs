
using System;
using System.Linq;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;

namespace DiversityPhone.Model
{

    [Table]
    public class IdentificationUnit : IModifyable
    {
        public IdentificationUnit()
        {
            this.ModificationState = null;
            this.LogUpdatedWhen = DateTime.Now;
            
            this.AnalysisDate = DateTime.Now;//TODO Something useful?
            this.RelatedUnitID = null;
        }

        [Column]
        public int SpecimenID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int UnitID { get; set; }

        [Column]
        public int? RelatedUnitID { get; set; }

        /// <summary>
        /// No part of the IU collected in physical Form. Always true for Observations
        /// </summary>
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
        /// <summary>
        /// Names as displayed on the screen
        /// </summary>
        [Column]
        public string WorkingName { get; set; }
       
        [Column]
        public string IdentificationUri { get; set; }


        //Georeferenzierung
        [Column]
        public double? Altitude { get; set; }

        [Column]
        public double? Latitude { get; set; }

        [Column]
        public double? Longitude { get; set; }

        [Column]
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

      public static IQueryOperations<IdentificationUnit> Operations
        {
            get;
            private set;
        }

        static IdentificationUnit()
        {
            Operations = new QueryOperations<IdentificationUnit>(
                //Smallerthan
                          (q, iu) => q.Where(row => row.UnitID < iu.UnitID),
                //Equals
                          (q, iu) => q.Where(row => row.UnitID == iu.UnitID),
                //Orderby
                          (q) => q.OrderBy(iu => iu.UnitID),
                //FreeKey
                          (q, iu) =>
                          {
                              iu.UnitID = QueryOperations<IdentificationUnit>.FindFreeIntKey(q, row => row.UnitID);
                          });
        }
    }
}
