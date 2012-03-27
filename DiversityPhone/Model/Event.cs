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


        [Column(CanBeNull = true)]
        public double? Altitude { get; set; }

        [Column(CanBeNull = true)]
        public double? Latitude { get; set; }

        [Column(CanBeNull = true)]
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

        #region Associations
        //private EntityRef<EventSeries> _EventSeries;
        //[Association(Name = "FK_Event_EventSeries",
        //        Storage = "_EventSeries",
        //        ThisKey = "SeriesID",
        //        OtherKey = "SeriesID",
        //        IsForeignKey = true)]
        //public EventSeries EventSeries //Directly applied example
        //{
        //    get { return _EventSeries.Entity; }
        //    set
        //    {
        //        EventSeries previousValue = this._EventSeries.Entity;
        //        if (((previousValue != value) ||
        //            (this._EventSeries.HasLoadedOrAssignedValue
        //             == false)))
        //        {
        //            if ((previousValue != null))
        //            {
        //                this._EventSeries.Entity = null;
        //                previousValue.Events.Remove(this);
        //            }
        //            this._EventSeries.Entity = value;
        //            if ((value != null))
        //            {
        //                value.Events.Add(this);
        //                this.SeriesID = value.SeriesID;
        //            }
        //            else
        //            {
        //                this.SeriesID = default(int);
        //            }
        //        }
        //    }
        //}

        //private EntitySet<Specimen> _Specimen;
        //[Association(Name = "FK_Event_Specimen",
        //             Storage = "_Specimen",
        //             ThisKey = "EventID",
        //             OtherKey = "CollectionEventID")]
        //public EntitySet<Specimen> Specimen
        //{
        //    get { return _Specimen; }
        //    set { _Specimen.Assign(value); }
        //}

        //private EntitySet<CollectionEventProperty> _Properties;
        //[Association(Name = "FK_Event_Property",
        //             Storage = "_Properties",
        //             ThisKey = "EventID",
        //             OtherKey = "EventID",
        //             IsForeignKey = true,
        //             DeleteRule = "CASCADE")]
        //public EntitySet<CollectionEventProperty> Properties
        //{
        //    get { return _Properties; }
        //    set { _Properties.Assign(value); }
        //}

        //private void Attach_Specimen(Specimen entity)
        //{
        //    entity.Event = this;
        //}

        //private void Detach_Specimen(Specimen entity)
        //{
        //    entity.Event = null;
        //}

        //private void Attach_Property(CollectionEventProperty entity)
        //{
        //    entity.Event = this;
        //}

        //private void Detach_Property(CollectionEventProperty entity)
        //{
        //    entity.Event = null;
        //}

        #endregion
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
