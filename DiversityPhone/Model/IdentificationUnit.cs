using System;
using System.Linq;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;


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
            this.DiversityCollectionUnitID = null;
            this.DiversityCollectionSpecimenID = null;
            this.DiversityCollectionRelatedUnitID = null;
        }

        [Column]
        public int SpecimenID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionSpecimenID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int UnitID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionUnitID { get; set; }

        [Column]
        public int? RelatedUnitID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionRelatedUnitID { get; set; }

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

        public static Svc.IdentificationUnit ConvertToServiceObject(IdentificationUnit iu)
        {
            Svc.IdentificationUnit export = new Svc.IdentificationUnit();
            export.DiversityCollectionUnitID = iu.DiversityCollectionUnitID;
            export.DiversityCollectionSpecimenID = iu.DiversityCollectionSpecimenID;
            export.DiversityCollectionRelatedUnitID = iu.DiversityCollectionRelatedUnitID;
            export.Altitude = iu.Altitude;
            export.AnalysisDate = iu.AnalysisDate;
            export.ColonisedSubstratePart = iu.ColonisedSubstratePart;
            //export.FamilyCache=iu. Is not supported on clientModel
            export.Gender = iu.Gender;
            export.IdentificationUri = iu.IdentificationUri;
            export.LastIdentificationCache = iu.WorkingName;
            export.Latitude = iu.Latitude;
            export.LifeStage = iu.LifeStage;
            export.LogUpdatedWhen = iu.LogUpdatedWhen;
            export.Longitude = iu.Longitude;
            export.OnlyObserved = iu.OnlyObserved;
            //export.OrderCache=iu.Is not supported on clientModel
            export.RelatedUnitID = iu.RelatedUnitID;
            export.RelationType = iu.RelationType;
            export.SpecimenID = iu.SpecimenID;
            export.TaxonomicGroup = iu.TaxonomicGroup;
            export.UnitID = iu.UnitID;
            return export;
        }


        //public static IdentificationUnit Clone(IdentificationUnit iu)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
