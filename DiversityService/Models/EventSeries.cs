using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DiversityService.Model
{

    public class EventSeries
    {   
        public int SeriesID { get; set; }
        public int? DiversityCollectionEventSeriesID { get; set; }
        public string Description { get; set; }
        public string SeriesCode { get; set; }
        public String Geography { get; set; }//Geostring for setting the Geography
        public DateTime? SeriesStart { get; set; }
        public DateTime? SeriesEnd { get; set; }

        public DateTime? LogUpdatedWhen { get; set; }

        
    }
}
