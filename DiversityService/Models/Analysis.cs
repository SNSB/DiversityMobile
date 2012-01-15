using System;
using PetaPoco;


namespace DiversityService.Model
{
    public class Analysis
    {
        //Read-Only
        [Column("AnalysisID")]
        public int AnalysisID { get; set; }
        [Column("DisplayText")]
        public String DisplayText { get; set; }
        [Column("Description")]
        public String Description { get; set; }
        [Column("MeasurementUnit")]
        public String MeasurementUnit { get; set; }
    }
}
