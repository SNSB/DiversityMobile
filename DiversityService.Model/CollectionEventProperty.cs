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
    public class CollectionEventProperty
    {
        public int EventID{get;set;}
        public int PropertyID { get; set; }
        public String DisplayText { get; set; }
        public String PropertyUri { get; set; }

        public DateTime LogUpdatedWhen { get; set; }
    }
}
