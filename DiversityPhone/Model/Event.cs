namespace DiversityPhone.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq.Mapping;
    using System.Linq;
    using System.Text;
    using Microsoft.Phone.Data.Linq.Mapping;

    [Table]
    public class Event 
    {
        public Event()
        {
            this.CollectionDate = DateTime.Now;
            this.DeterminationDate = DateTime.Now;
            this.IsModified = null;
        }

        [Column(IsPrimaryKey = true)]
        public int EventID { get; set; }

        [Column]        
        public int SeriesID { get; set; }

        [Column]
        public DateTime CollectionDate { get; set; }

        [Column]
        public string LocalityDescription { get; set; }

        [Column]
        public string HabitatDescription { get; set; }


        [Column]   
        public double Altitude { get; set; }

        [Column]   
        public double Latitude { get; set; }

        [Column]   
        public double Longitude { get; set; }

        [Column]
        public DateTime DeterminationDate { get; set; }

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

    }
}
