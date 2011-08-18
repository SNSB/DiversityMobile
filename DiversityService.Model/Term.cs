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
    public class Term
    {
        public string Code { get; set; }
        public int SourceID { get; set; } 
        public string Description { get; set; }
        public string DisplayText { get; set; }
        public string ParentCode { get; set; }
    }
}
