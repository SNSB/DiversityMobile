using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    public class AnalysisTaxonomicGroup
    {
        public AnalysisTaxonomicGroup()
        {
            //LogUpdatedWhen = DateTime.Now;
        }

        //Read-Only
        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column(IsPrimaryKey = true)]
        public string TaxonomicGroup { get; set; }

        //[Column]
        //public DateTime LogUpdatedWhen { get; set; }
    }
}