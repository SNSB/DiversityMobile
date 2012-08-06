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
using ReactiveUI;

    [Table]
    public class Event : ReactiveObject, IModifyable, ILocalizable, IMultimediaOwner
    {
        public Event()
        {
            this.SeriesID = null;
            this.CollectionDate = DateTime.Now;
            this.LogUpdatedWhen = DateTime.Now;
            this.ModificationState = ModificationState.New;
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


        private string _LocalityDescription;      

        [Column]
        public string LocalityDescription
        {
            get { return _LocalityDescription; }
            set { this.RaiseAndSetIfChanged(x => x.LocalityDescription, ref _LocalityDescription, value); }
        }

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
        /// Tracks modifications to this Object        
        /// </summary>
        [Column]
        public ModificationState ModificationState { get; set; }

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
            if (ev.DiversityCollectionEventID.HasValue)
                export.DiversityCollectionEventID = ev.DiversityCollectionEventID.Value;
            else
                export.DiversityCollectionEventID = Int32.MinValue;
            export.DiversityCollectionSeriesID = ev.DiversityCollectionSeriesID;
            export.Altitude = ev.Altitude;
            export.CollectionDate = ev.CollectionDate;
            export.DeterminationDate = ev.DeterminationDate;
            export.EventID = ev.EventID;
            export.HabitatDescription = ev.HabitatDescription;
            export.Latitude = ev.Latitude;
            export.LocalityDescription = ev.LocalityDescription;
            export.LogUpdatedWhen = ev.LogUpdatedWhen;
            export.Longitude = ev.Longitude;
            export.SeriesID = ev.SeriesID;
            return export;
        }

        public ReferrerType OwnerType
        {
            get { return ReferrerType.Event; }
        }


        public int OwnerID
        {
            get { return EventID; }
        }
    }
}
