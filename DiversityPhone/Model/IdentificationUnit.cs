using System;
using System.Linq;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;
using System.Data.Linq;


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
            _RelatedUnits = new EntitySet<IdentificationUnit>(
              new Action<IdentificationUnit>(Attach_RelatedUnit),
              new Action<IdentificationUnit>(Detach_RelatedUnit));
            _IUAnalyses = new EntitySet<IdentificationUnitAnalysis>(
              new Action<IdentificationUnitAnalysis>(Attach_Analysis),
              new Action<IdentificationUnitAnalysis>(Detach_Analysis));
            _Specimen = default(EntityRef<Specimen>);
            _RelatedUnit = default(EntityRef<IdentificationUnit>);
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

        
        #region Associations
        private EntitySet<IdentificationUnit> _RelatedUnits;
        [Association(Name = "FK_Unit_RelatedUnits",
                     Storage = "_RelatedUnits",
                     ThisKey = "UnitID",
                     OtherKey = "RelatedUnitID",
                     IsForeignKey = true,
                     DeleteRule = "CASCADE")]
        public EntitySet<IdentificationUnit> Units
        {
            get { return _RelatedUnits; }
            set { _RelatedUnits.Assign(value); }
        }

        private EntitySet<IdentificationUnitAnalysis> _IUAnalyses;
        [Association(Name = "FK_Unit_Analyses",
                     Storage = "_IUAnalyses",
                     ThisKey = "UnitID",
                     OtherKey = "IdentificationUnitID",
                     IsForeignKey = true,
                     DeleteRule = "CASCADE")]
        public EntitySet<IdentificationUnitAnalysis> IUAnalyses
        {
            get { return _IUAnalyses; }
            set { _IUAnalyses.Assign(value); }
        }

        private EntityRef<Specimen> _Specimen;
        [Association(Name = "FK_Unit_Specimen",
                Storage = "_Specimen",
                ThisKey = "SpecimenID",
                OtherKey = "CollectionSpecimenID",
                IsForeignKey = true)]
        public Specimen Specimen
        {
            get { return _Specimen.Entity; }
            set
            {
                Specimen previousValue = this._Specimen.Entity;
                if (((previousValue != value) ||
                    (this._Specimen.HasLoadedOrAssignedValue
                     == false)))
                {
                    if ((previousValue != null))
                    {
                        this._Specimen.Entity = null;
                        previousValue.Units.Remove(this);
                    }
                    this._Specimen.Entity = value;
                    if ((value != null))
                    {
                        value.Units.Add(this);
                        this.SpecimenID = value.CollectionSpecimenID;
                    }
                    else
                    {
                        this.SpecimenID = default(int);
                    }
                }
            }
        }

        private EntityRef<IdentificationUnit> _RelatedUnit;
        [Association(Name = "FK_RelatedUnit_Unit",
                Storage = "_RelatedUnit",
                ThisKey = "UnitID",
                OtherKey = "RelatedUnitID",
                IsForeignKey = true)]
        public IdentificationUnit RelatedUnit
        {
            get { return _RelatedUnit.Entity; }
            set
            {
                IdentificationUnit previousValue = this._RelatedUnit.Entity;
                if (((previousValue != value) ||
                    (this._RelatedUnit.HasLoadedOrAssignedValue
                     == false)))
                {
                    if ((previousValue != null))
                    {
                        this._RelatedUnit.Entity = null;
                        previousValue.Units.Remove(this);
                    }
                    this._RelatedUnit.Entity = value;
                    if ((value != null))
                    {
                        value.Units.Add(this);
                        this.RelatedUnitID = value.UnitID;
                    }
                    else
                    {
                        this.RelatedUnitID = default(int);
                    }
                }
            }
        }

        private void Attach_RelatedUnit(IdentificationUnit entity)
        {
            entity.RelatedUnit = this;
        }

        private void Detach_RelatedUnit(IdentificationUnit entity)
        {
            entity.RelatedUnit = null;
        }

        private void Attach_Analysis(IdentificationUnitAnalysis entity)
        {
            entity.Unit = this;
        }

        private void Detach_Analysis(IdentificationUnitAnalysis entity)
        {
            entity.Unit = null;
        }

        #endregion
      

        //public static IdentificationUnit Clone(IdentificationUnit iu)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
