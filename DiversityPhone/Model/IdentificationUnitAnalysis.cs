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
        
        [Column]
        public string AnalysisResult { get; set; }

        [Column]
        public DateTime AnalysisDate { get; set; } //Datum mit Uhrzeit

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = false)]
        public bool? IsModified { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }


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
    }
}
