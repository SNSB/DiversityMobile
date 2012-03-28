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
        [DataMember]
        public int? DiversityCollectionEventSeriesID { get; set; }
        public string Description { get; set; }
        public string SeriesCode { get; set; }
        [DataMember]
        public String Geography { get; set; }//Geostring for setting the Geography
        [DataMember]
        public DateTime? SeriesStart { get; set; }
        [DataMember]
        public DateTime? SeriesEnd { get; set; }
        [DataMember]
        public DateTime? LogUpdatedWhen { get; set; }

        
    }
}
