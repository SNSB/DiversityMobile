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
    public class Specimen
    {
        public int CollectionSpecimenID{get; set;}
        public int CollectionEventID { get; set; }
        public string AccesionNumber { get; set; }
        public DateTime AccessionDate { get; set; }
    }
}
