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
    public class AnalysisTaxonomicGroup
    {
        public AnalysisTaxonomicGroup()
        {
            LogUpdatedWhen = DateTime.Now;
        }

        //Read-Only
        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column(IsPrimaryKey = true)]
        public string TaxonomicGroup { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

    }
}
