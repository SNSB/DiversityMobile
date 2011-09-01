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
    public class TaxonName
    {
        public string URI { get; set; }
        public string TaxonNameCache { get; set; }
        public string TaxonNameSinAuth { get; set; }
        public string GenusOrSupragenic { get; set; }
        public string SpeciesEpithet { get; set; }
        public string InfraspecificEpithet { get; set; }
    }
}
