using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    
    public class EventSeries
    {       
        public int SeriesID { get; set; }
        public string Description { get; set; }
        public string SeriesCode { get; set; }
        public String Geography { get; set; }//Todo:Adjsutments for handling
        public DateTime? SeriesStart { get; set; }
        public DateTime? SeriesEnd { get; set; }

        public DateTime? LogUpdatedWhen { get; set; }

        
    }
}
