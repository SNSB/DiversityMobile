using System;

namespace DiversityService.Model
{
    public class AnalysisTaxonomicGroup
    {
        //Read-Only
        public int AnalysisID { get; set; }
        public string TaxonomicGroup { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(AnalysisTaxonomicGroup))
            {
                var other = (AnalysisTaxonomicGroup)obj;
                return this.AnalysisID == other.AnalysisID && this.TaxonomicGroup == other.TaxonomicGroup;
            }
            else
                return base.Equals(obj);
        }

    }
}
