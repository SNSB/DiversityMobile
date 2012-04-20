using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace DiversityService.Model
{
    public class CollectionEventLocalisation
    {
        public int CollectionEventID { get; set; }
        public int LocalisationSystemID { get; set; }
        public String Location1 { get; set; }
        public String Location2 { get; set; }
        public DateTime? DeterminationDate { get; set; }
        public String ResponsibleName { get; set; }
        public String ResponsibleAgentURI { get; set; }
        public String RecordingMethod { get; set; }

        [Ignore]
        public String Geography { get; set; }
        public double? AverageAltitudeCache { get; set; }
        public double? AverageLatitudeCache { get; set; }
        public double? AverageLongitudeCache { get; set; }

    }
}
