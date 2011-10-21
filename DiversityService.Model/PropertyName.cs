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
    public class PropertyName
    {
        public int PropertyID { get; set; }
        public int TermID { get; set; }
        public int BroaderTermID { get; set; }
        public string PropertyUri { get; set; }
        public string DisplayText { get; set; }
        public string HierarchyCache { get; set; }

    }
}
