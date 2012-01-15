using System;
using PetaPoco;


namespace DiversityService.Model
{
    public class AnalysisResult
    {
        //Read-Only
        public int AnalysisID { get; set; }
        [Column("AnalysisResult")]
        public string Result { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string DisplayText { get; set; }      
    }
}
