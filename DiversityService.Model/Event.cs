using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    
    public class Event
    {
        public int EventID { get; set; }
        public int SeriesID { get; set; }
        public DateTime CollectionDate { get; set; }
        public string LocalityDescription { get; set; }
        public string HabitatDescription { get; set; }

        public bool? IsModified { get; set; }

        public Event()
        {
            CollectionDate = DateTime.Now;
            IsModified = null;
        }
    }
}
