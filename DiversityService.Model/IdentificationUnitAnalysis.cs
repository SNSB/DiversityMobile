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
