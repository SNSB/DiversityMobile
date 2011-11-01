using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DiversityService.Model
{
    
    public class HierarchySection
    { 
        public Event Event { get; set; }

        public IList<Specimen> Specimen { get; set; }

        public IList<IdentificationUnit> IdentificationUnits { get; set; }
    }
}
