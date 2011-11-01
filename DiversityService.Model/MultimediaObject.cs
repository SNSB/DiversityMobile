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
    public class MultimediaObject
    {
        public int SourceId { get; set; }
        public int RelatedId { get; set; }
        public String Uri {get;set;}
        public String MediaType { get; set; }
        public DateTime LogUpdatedWhen { get; set; }
    }
}
