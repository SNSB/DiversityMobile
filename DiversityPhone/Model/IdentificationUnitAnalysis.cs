using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;
using System.Data.Linq;


namespace DiversityPhone.Model
{
    [Table]
    public class IdentificationUnitAnalysis : IModifyable
    {
        [Column]
        public int IdentificationUnitID { get; set; }

        [Column]
        public int AnalysisID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int IdentificationUnitAnalysisID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionUnitID { get; set; }

        [Column]
        public string AnalysisResult { get; set; }

        [Column]
        public DateTime AnalysisDate { get; set; } //Datum mit Uhrzeit

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = false)]
        public bool? ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

        public IdentificationUnitAnalysis()
        {
            this.ModificationState = null;
            this.LogUpdatedWhen = DateTime.Now;
            this.AnalysisDate = DateTime.Now;
            this.DiversityCollectionUnitID = null;
            _Unit = default(EntityRef<IdentificationUnit>);
        }

        public static IQueryOperations<IdentificationUnitAnalysis> Operations
        {
            get;
            private set;
        }

        static IdentificationUnitAnalysis()
        {
            Operations = new QueryOperations<IdentificationUnitAnalysis>(
                //Smallerthan
                          (q, iuan) => q.Where(row => row.IdentificationUnitAnalysisID < iuan.IdentificationUnitAnalysisID),
                //Equals
                          (q, iuan) => q.Where(row => row.IdentificationUnitAnalysisID == iuan.IdentificationUnitAnalysisID),
                //Orderby
                          (q) => q.OrderBy(iuan => iuan.IdentificationUnitAnalysisID),
                //FreeKey
                          (q, iuan) =>
                          {
                              iuan.IdentificationUnitAnalysisID = QueryOperations<IdentificationUnitAnalysis>.FindFreeIntKey(q, row => row.IdentificationUnitAnalysisID);
                          });
        }

        public static Svc.IdentificationUnitAnalysis ConvertToServiceObject(IdentificationUnitAnalysis iua)
        {
            Svc.IdentificationUnitAnalysis export = new Svc.IdentificationUnitAnalysis();
            export.DiversityCollectionUnitID = iua.DiversityCollectionUnitID;
            export.AnalysisDate = iua.AnalysisDate;
            export.AnalysisID = iua.AnalysisID;
            export.AnalysisResult = iua.AnalysisResult;
            export.IdentificationUnitAnalysisID = iua.IdentificationUnitAnalysisID;
            export.IdentificationUnitID = iua.IdentificationUnitID;
            return export;
        }

        #region Associations

        private EntityRef<IdentificationUnit> _Unit;
        [Association(Name = "FK_IUA_Unit",
                Storage = "_Unit",
                ThisKey = "IdentificationUnitID",
                OtherKey = "UnitID",
                IsForeignKey = true)]
        public IdentificationUnit Unit
        {
            get { return _Unit.Entity; }
            set
            {
                IdentificationUnit previousValue = this._Unit.Entity;
                if (((previousValue != value) ||
                    (this._Unit.HasLoadedOrAssignedValue
                     == false)))
                {
                    if ((previousValue != null))
                    {
                        this._Unit.Entity = null;
                        previousValue.IUAnalyses.Remove(this);
                    }
                    this._Unit.Entity = value;
                    if ((value != null))
                    {
                        value.IUAnalyses.Add(this);
                        this.IdentificationUnitID = value.UnitID;
                    }
                    else
                    {
                        this.IdentificationUnitID = default(int);
                    }
                }
            }
        }

        #endregion
    }
}