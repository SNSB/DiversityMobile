using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DiversityService.Model
{
    [DataContract(Name = "EventSeries", Namespace = "http://tempuri.org")]
    public class EventSeries
    {   
        [DataMember]
        public int SeriesID { get; set; }
        [DataMember]
        public int? DiversityCollectionEventSeriesID { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
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
