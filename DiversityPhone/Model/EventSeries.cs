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
            this.SeriesEnd = DateTime.Now;

            this.IsModified = null;
        }

        [Column(IsPrimaryKey = true)]
        public int SeriesID { get; set; }

        [Column(CanBeNull = false)]
        public string Description { get; set; }

        [Column]
        public string SeriesCode { get; set; }

        [Column]
        public DateTime SeriesStart { get; set; }

        [Column]
        public DateTime SeriesEnd { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }
        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }

   
    }
}
