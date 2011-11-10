using System;
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

namespace DiversityPhone.Model
{
    [Table]
    public class IdentificationUnitAnalysis
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
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }
    }
}
