

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
    public class GeoPointForSeries : IModifyable, ILocalizable
    {
        //Stores Geocoordinates assiciated with an EventSeries. Only one tour per series are allowed. Sequence of points is given by the id.


        [Column]
        public int SeriesID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int PointID{get;set;}


        [Column(CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public double? Latitude { get; set; }


        [Column(CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public double? Longitude { get; set; }


        [Column(CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
        public double? Altitude { get; set; }

        /// <summary>
        /// Tracks modifications to this Object.        
        /// </summary>
        [Column]
        public ModificationState ModificationState { get; set; }
        
        public static IQueryOperations<GeoPointForSeries> Operations
        {
            get;
            private set;
        }

        static GeoPointForSeries()
        {
            Operations = new QueryOperations<GeoPointForSeries>(
                //Smallerthan
                          (q, gt) => q.Where(row => row.PointID < gt.PointID),
                //Equals
                          (q, gt) => q.Where(row => row.PointID == gt.PointID),
                //Orderby
                          (q) => q.OrderBy(gt => gt.PointID),
                //FreeKey
                          (q, gt) =>
                          {
                              gt.PointID = QueryOperations<GeoPointForSeries>.FindFreeIntKeyUp(q, row => row.PointID);//As there is no corredponding table in DiversityCollection we choose to count up to model a sequence
                          });
        }

    }
}
