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
    public class AnalysisResult
    {
        //Read-Only
        public int AnalysisID { get; set; }
        public string Result { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string DisplayText { get; set; }
        public DateTime LogUpdatedWhen { get; set; }
    }
}
