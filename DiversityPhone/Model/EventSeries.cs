namespace DiversityPhone.Model
{
    using System;
    using System.Linq;
    using System.Data.Linq.Mapping;
    using DiversityPhone.Services;

    [Table]
    public class EventSeries 
    {

        public EventSeries()
        {
            this.Description = string.Empty;
            this.SeriesCode = string.Empty;
            this.SeriesStart = DateTime.Now;
            this.SeriesEnd = null;
            this.SeriesID = 0;
            this.LogUpdatedWhen = DateTime.Now;
            this.IsModified = null;
        }


        public static EventSeries NoEventSeries()
        {
            EventSeries noEs = new EventSeries();
            noEs.Description = "Go to Events";
            noEs.SeriesCode = "Go to Events";
            noEs.IsModified = false;
            return noEs;
        }

        public static bool isNoEventSeries(EventSeries es)
        {
            EventSeries noes = EventSeries.NoEventSeries();
            if (es.Description.Equals(noes.Description))
                return true;
            else
                return false;
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
                          }));
        }
        

    }
}
