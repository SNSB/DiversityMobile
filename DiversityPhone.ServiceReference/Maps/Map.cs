using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    public class Map
    {

        [Column(IsPrimaryKey = true)]
        public string ServerKey { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Description { get; set; }

        [Column]
        public double NWLat { get; set; }

        [Column]
        public double NWLong { get; set; }

        [Column]
        public double SELat { get; set; }

        [Column]
        public double SELong { get; set; }

        [Column]
        public double SWLat { get; set; }

        [Column]
        public double SWLong { get; set; }
        
        [Column]
        public double NELat { get; set; }

        [Column]
        public double NELong { get; set; }
       
        [Column(CanBeNull = true)]
        public int? Transparency{get;set;}

        [Column(CanBeNull = true)]
        public int? ZoomLevel{ get; set; }
    }
}
