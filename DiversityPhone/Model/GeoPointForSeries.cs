

namespace DiversityPhone.Model
{
    using System;
    using System.Linq;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using DiversityPhone.Services;
    using Svc = DiversityPhone.DiversityService;

    [Table]
    public class GeoPointForSeries :IModifyable
    {
        //Dtores Geocoordinates assiciated with an EventSeries. Only one tour per series are allowed. Sequence of points is given by the id.


        [Column]
        public int SeriesID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int PointID{get;set;}

        [Column(CanBeNull = true)]
        public double? Latitude;

        [Column(CanBeNull = true)]
        public double? Longitude;

        [Column(CanBeNull = true)]
        public double? Altitude;

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? ModificationState { get; set; }
        
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
