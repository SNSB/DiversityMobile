using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace DiversityService.Model
{
    [TableName("CollectionEvent")]
    [PrimaryKey("CollectionEventID", autoIncrement = true)]
    public class Event
    {
        [Ignore]
        public int EventID { get; set; }

        [Column("CollectionEventID")]
        public int DiversityCollectionEventID { get; set; }

        [Ignore]
        public int? SeriesID { get; set; }

        [Column("SeriesID")]
        public int? DiversityCollectionSeriesID { get; set; }

        public DateTime CollectionDate { get; set; }
        public string LocalityDescription { get; set; }
        public string HabitatDescription { get; set; } 

        //Georeferenzierung anstelle der KLasse CollectionEventLocalisation
        //Altitude hat LocalisationSystem 4
        //WGS84 hat LocalisationSystem 8
        [Ignore]
        public double? Altitude { get; set; }
        [Ignore]
        public double? Latitude { get; set; }
        [Ignore]
        public double? Longitude { get; set; }
        [Ignore]
        public DateTime? DeterminationDate { get; set; }
        [Ignore]
        public DateTime LogUpdatedWhen { get; set; }

    }
}
