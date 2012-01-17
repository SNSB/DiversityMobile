namespace DiversityPhone.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq.Mapping;
    using System.Linq;
    using System.Text;
    using Microsoft.Phone.Data.Linq.Mapping;
    using DiversityPhone.Services;

    [Table]
    public class Event : IModifyable
    {
        public Event()
        {
            this.SeriesID = null;
            this.CollectionDate = DateTime.Now;
            this.LogUpdatedWhen = DateTime.Now;
            this.ModificationState = null;

            if (UserProfile.Actual != null && UserProfile.Actual.RecordGeoPosition == true)
            {
                if (App.Watcher != null)
                {
                    this.DeterminationDate = DateTime.Now;
                    this.Latitude = App.Watcher.Position.Location.Latitude;
                    this.Longitude = App.Watcher.Position.Location.Longitude;
                    this.Altitude = App.Watcher.Position.Location.Altitude;
                }
            }
        }

        [Column(IsPrimaryKey = true)]
        public int EventID { get; set; }

        [Column(CanBeNull=true)]        
        public int? SeriesID { get; set; }

        [Column]
        public DateTime CollectionDate { get; set; }

        [Column]
        public string LocalityDescription { get; set; }

        [Column]
        public string HabitatDescription { get; set; }


        [Column(CanBeNull = true)]
        public double? Altitude { get; set; }

        [Column(CanBeNull = true)]
        public double? Latitude { get; set; }

        [Column(CanBeNull = true)]
        public double? Longitude { get; set; }

        [Column(CanBeNull=true)]
        public DateTime? DeterminationDate { get; set; }

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }


       public static IQueryOperations<Event> Operations
        {
            get;
            private set;
        }

        static Event()
        {
            Operations = new QueryOperations<Event>(
                //Smallerthan
                          (q, ev) => q.Where(row => row.EventID < ev.EventID),
                //Equals
                          (q, ev) => q.Where(row => row.EventID == ev.EventID),
                //Orderby
                          (q) => q.OrderBy(ev => ev.EventID),
                //FreeKey
                          (q, ev) =>
                          {
                              ev.EventID = QueryOperations<Event>.FindFreeIntKey(q, row => row.EventID);
                          });
        }
    }
}
