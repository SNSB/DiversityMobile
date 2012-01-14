namespace DiversityPhone.Model
{
    using System;
    using System.Linq;
    using System.Data.Linq.Mapping;
    using DiversityPhone.Services;

    [Table]
    public class EventSeries : IModifyable
    {
        public const string NOEVENT_CONTEXT = "NoEventSeries";

        private static EventSeries _NoEventSeries;
      
        public EventSeries()
        {
            this.Description = string.Empty;
            this.SeriesCode = string.Empty;
            this.SeriesStart = DateTime.Now;
            this.SeriesEnd = null;
            this.SeriesID = 0;
            this.LogUpdatedWhen = DateTime.Now;
            this.ModificationState = null;
        }


        public static EventSeries NoEventSeries
        {
            get
            {
                return _NoEventSeries;
            }
        }

        public static bool isNoEventSeries(EventSeries es)
        {
            return _NoEventSeries == es;
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
        public bool? ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }



        public static IQueryOperations<EventSeries> Operations
        {
            get;
            private set;
        }

        static EventSeries()
        {
            Operations = new QueryOperations<EventSeries>(
                //Smallerthan
                          (q, es) => q.Where(row => row.SeriesID < es.SeriesID),
                //Equals
                          (q, es) => q.Where(row => row.SeriesID == es.SeriesID),
                //Orderby
                          (q) => q.OrderBy(es => es.SeriesID),
                //FreeKey
                          (q, es) =>
                          {
                              es.SeriesID = QueryOperations<EventSeries>.FindFreeIntKey(q, row => row.SeriesID);
                          });

            _NoEventSeries = new EventSeries()
            {
                Description = "No EventSeries",
                SeriesCode = "No EventSeries",
                ModificationState = false
            };
        }

        public static EventSeries Actual { get; set; }
    }
}
