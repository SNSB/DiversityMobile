using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DiversityService.Model
{
    
    public class HierarchySection
    {
        
        IList<EventSeries> EventSeries { get; set; }

        
        IList<Event> Events { get; set; }

        IList<Specimen> Specimen { get; set; }

        IList<IdentificationUnit> IdentificationUnits { get; set; }
    }
}
