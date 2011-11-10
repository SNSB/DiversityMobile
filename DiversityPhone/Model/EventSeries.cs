namespace DiversityPhone.Model
{
    using System;
    using System.Data.Linq.Mapping;

    [Table]
    public class EventSeries 
    {
        public EventSeries()
        {
            this.Description = string.Empty;
            this.SeriesCode = string.Empty;
            this.SeriesStart = DateTime.Now;
            this.SeriesEnd = null;
            this.LogUpdatedWhen = DateTime.Now;
            this.IsModified = null;
        }


        public static EventSeries NoEventSeries()
        {
            EventSeries noEs = new EventSeries();
            noEs.SeriesID = 0;
            noEs.Description = "Go to Events";
            noEs.SeriesCode = "Go to Events";
            noEs.IsModified = false;
            return noEs;
        }

        [Column(IsPrimaryKey = true)]
        public int SeriesID { get; set; }

        [Column(CanBeNull = false)]
        public string Description { get; set; }

        [Column]
        public string SeriesCode { get; set; }

        [Column]
        public DateTime SeriesStart { get; set; }

        [Column(CanBeNull = true)]
        public DateTime? SeriesEnd { get; set; }

        
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
