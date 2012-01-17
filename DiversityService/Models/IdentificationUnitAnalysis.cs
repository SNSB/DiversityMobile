using System;


namespace DiversityService.Model
{
    public class IdentificationUnitAnalysis
    {
        public int IdentificationUnitID { get; set; }
        public int AnalysisID { get; set; }
        public int IdentificationUnitAnalysisID { get; set; }
        public string AnalysisResult { get; set; }
        public DateTime AnalysisDate { get; set; } //Datum mit Uhrzeit
    }
}
