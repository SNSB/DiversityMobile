using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System;
using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    public class Map
    {

        [Column(IsPrimaryKey = true)]
        public String Uri { get; set; }

        [Column]
        public String Name { get; set; }

        [Column]
        public double LatitudeNorth { get; set; }

        [Column]
        public double LatitudeSouth { get; set; }

        [Column]
        public double LongitudeWest { get; set; }

        [Column]
        public double LongitudeEast { get; set; }

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

        public Map()
        {
            this.IsModified = null;
            this.LogUpdatedWhen = DateTime.Now;
        }
    }
}
