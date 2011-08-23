using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    
    public class EventSeries
    {
        public EventSeries()
        {
            Description = "";
            SeriesCode = "";
            SeriesStart = DateTime.Now;
            SeriesEnd = DateTime.Now;

            IsModified = null;
        }

        public int SeriesID { get; set; }
        public string Description { get; set; }
        public string SeriesCode { get; set; }

        public DateTime SeriesStart { get; set; }
        public DateTime SeriesEnd { get; set; }



        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        public bool? IsModified { get; set; }  
    }
}
