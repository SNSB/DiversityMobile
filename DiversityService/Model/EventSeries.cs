using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using PetaPoco;

namespace DiversityService.Model
{
    [TableName("CollectionEventSeries")]
    [PrimaryKey("SeriesID", autoIncrement = true)]
    public class EventSeries
    {
        [Ignore]
        public int SeriesID { get; set; }
        
        [Column("SeriesID")]
        public int DiversityCollectionEventSeriesID { get; set; }
       
        public string Description { get; set; }
        
        public string SeriesCode { get; set; }

        [Ignore]
        public String Geography { get; set; }//Geostring for setting the Geography
       
        [Column("DateStart")]
        public DateTime? SeriesStart { get; set; }

        [Column("DateEnd")]
        public DateTime? SeriesEnd { get; set; }
        
        [Ignore]
        public DateTime? LogUpdatedWhen { get; set; }

        
    }
}
