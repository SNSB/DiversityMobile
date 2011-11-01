using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DiversityService.Model
{
    
    public class HierarchySection
    {        
        public EventSeries EventSeries { get; set; }

        public IList<Event> Events { get; set; }

        public IList<Specimen> Specimen { get; set; }

        public IList<IdentificationUnit> IdentificationUnits { get; set; }
    }
}
