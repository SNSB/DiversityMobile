namespace DiversityPhone.Model
{
    using System;
    using System.Linq;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using DiversityPhone.Services;
    using Svc = DiversityPhone.DiversityService;
using ReactiveUI;

    [Table]
    public class EventSeries : ReactiveObject, IModifyable
    {
        private static EventSeries _NoEventSeries;

        public EventSeries()
        {
            this.Description = string.Empty;
            this.SeriesCode = string.Empty;
            this.SeriesStart = DateTime.Now;
            this.SeriesEnd = null;
            this.SeriesID = 0;
            this.LogUpdatedWhen = DateTime.Now;
            this.ModificationState = ModificationState.New;
            this.DiversityCollectionEventSeriesID = null;
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
        public int? SeriesID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionEventSeriesID { get; set; }

        private string _Description;
        [Column(CanBeNull = false)]
        public string Description 
        {
            get { return _Description; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Description, ref _Description, value);
            }
        }

        [Column]
        public string SeriesCode { get; set; }

        [Column]
        public DateTime SeriesStart { get; set; }

        [Column(CanBeNull = true)]
        public DateTime? SeriesEnd { get; set; }



        ModificationState _ModificationState;
        [Column]
        public ModificationState ModificationState 
        {
            get { return _ModificationState; }
            set { this.RaiseAndSetIfChanged(x => x.ModificationState, ref _ModificationState, value); }
        }

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
                          (q) => q.OrderBy(es => -es.SeriesID),//As EventSeries have negative Values Unlike the other entities in DiversityCollection we need to order by the nagtive value to get the same behaviour
                //FreeKey
                          (q, es) =>
                          {
                              es.SeriesID = QueryOperations<EventSeries>.FindFreeIntKeyUp(q, row => row.SeriesID.Value);//CollectionEventSeries Autoinc-key is lowered by one by default in DiversityCollection. As we need to avoid Synchronisationconflicts we need to count in the other direction.
                          });

            _NoEventSeries = new EventSeries()
            {
                Description = "Events",
                SeriesCode = "No EventSeries",
                ModificationState = ModificationState.Unmodified,
                SeriesEnd = DateTime.Now,
                SeriesID = null
            };
        }

        public static Svc.EventSeries ToServiceObject(EventSeries es)
        {
            Svc.EventSeries export = new Svc.EventSeries();
            export.SeriesID = es.SeriesID.Value;
            if (es.DiversityCollectionEventSeriesID != null)
                export.DiversityCollectionEventSeriesID = (int)es.DiversityCollectionEventSeriesID;
            else
                export.DiversityCollectionEventSeriesID = Int32.MinValue;
            export.SeriesCode = es.SeriesCode;
            export.SeriesStart = es.SeriesStart;
            export.SeriesEnd = es.SeriesEnd;
            export.Description = es.Description;
            export.LogUpdatedWhen = es.LogUpdatedWhen;
            return export;
        }
    }
}
