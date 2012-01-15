using System;


namespace DiversityService.Model
{
    public class Analysis
    {
        //Read-Only

        public int AnalysisID { get; set; }
        public String DisplayText { get; set; }
        public String Description { get; set; }
        public String MeasurementUnit { get; set; }
        public DateTime LogUpdatedWhen { get; set; }

    }
}
