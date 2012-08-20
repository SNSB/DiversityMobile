using System;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
using System.Windows;
using DiversityPhone.Model.Geometry;

namespace DiversityPhone.Model
{
    [Table]
    public class Map : IModifyable
    {

        [Column(IsPrimaryKey = true)]
        public String ServerKey { get; set; }

        [Column]
        public String Uri { get; set; }

        [Column]
        public String Name { get; set; }

        [Column]
        public String Description { get; set; }

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

      

        /// <summary>
        /// Tracks modifications to this Object.        
        /// </summary>
        [Column]
        public ModificationState ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

        public Map()
        {
            this.ModificationState = ModificationState.New;
            this.LogUpdatedWhen = DateTime.Now;
        }
    }
}
