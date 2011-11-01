using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    
    public class Event
    {
        public int EventID { get; set; }
        public int? SeriesID { get; set; }
        public DateTime CollectionDate { get; set; }
        public string LocalityDescription { get; set; }
        public string HabitatDescription { get; set; } 

        //Georeferenzierung anstelle der KLasse CollectionEventLocalisation
        //Altitude hat LocalisationSystem 4
        //WGS84 hat LocalisationSystem 8
        public double Altitude { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime DeterminationDate { get; set; }

        public DateTime LogUpdatedWhen { get; set; }

    }
}
