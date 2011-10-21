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
        //Read-Only

        public virtual string URI { get; set; }
        public virtual string TaxonNameCache { get; set; }
        public virtual string TaxonNameSinAuth { get; set; }
        public virtual string GenusOrSupragenic { get; set; }
        public virtual string SpeciesEpithet { get; set; }
        public virtual string InfraspecificEpithet { get; set; }
        public virtual string Synonymy { get; set; }
    }
}
