namespace DiversityPhone.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq.Mapping;
    using System.Linq;
    using System.Text;
    using Microsoft.Phone.Data.Linq.Mapping;
    using DiversityPhone.Services;
    using System.Data.Linq;
    using Svc = DiversityPhone.DiversityService;

    [Table]
    public class Event : IModifyable, ILocalizable
    {
        public Event()
        {
            this.SeriesID = null;
            this.CollectionDate = DateTime.Now;
            this.LogUpdatedWhen = DateTime.Now;
            this.ModificationState = null;
            this.DiversityCollectionEventID = null;
            this.DiversityCollectionSeriesID = null;
            

        }

        [Column(IsPrimaryKey = true)]
        public int EventID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionEventID { get; set; }

        [Column(CanBeNull = true)]
        public int? SeriesID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionSeriesID { get; set; }

        [Column]
        public DateTime CollectionDate { get; set; }

        [Column]
        public string LocalityDescription { get; set; }

        [Column]
        public string HabitatDescription { get; set; }


        [Column(CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public double? Altitude { get; set; }

        [Column(CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public double? Latitude { get; set; }

        [Column(CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public double? Longitude { get; set; }

        [Column(CanBeNull = true)]
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

        public static Svc.Event ConvertToServiceObject(Event ev)
        {
            Svc.Event export = new Svc.Event();
            export.DiversityCollectionEventID = ev.DiversityCollectionEventID;
            export.DiversityCollectionSeriesID = ev.DiversityCollectionSeriesID;
            export.Altitude = ev.Altitude;
            export.CollectionDate = ev.CollectionDate;
            export.EventID = ev.EventID;
            export.HabitatDescription = ev.HabitatDescription;
            export.Latitude = ev.Latitude;
            export.LocalityDescription = ev.LocalityDescription;
            export.LogUpdatedWhen = ev.LogUpdatedWhen;
            export.Longitude = ev.Longitude;
            export.SeriesID = ev.SeriesID;
            return export;
        }
    }
}
