using System;

namespace DiversityService.Model
{
    public class AnalysisTaxonomicGroup
    {
        //Read-Only
        public int AnalysisID { get; set; }
        public string TaxonomicGroup { get; set; }
        public DateTime LogUpdatedWhen { get; set; }

    }
}
