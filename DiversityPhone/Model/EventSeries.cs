using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    
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

        [Column(IsPrimaryKey=true)]
        public int SeriesID { get; set; }

        [Column(CanBeNull=false)]
        public string Description { get; set; }

        [Column]
        public string SeriesCode { get; set; }
        
        [Column]
        public DateTime SeriesStart { get; set; }

        [Column]
        public DateTime SeriesEnd { get; set; }



        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull=true)]
        public bool? IsModified { get; set; }  
    }
}
