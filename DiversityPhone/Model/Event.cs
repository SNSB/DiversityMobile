using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using ReactiveUI;
using Microsoft.Phone.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    [Index(Columns="CollectionDate",IsUnique=false, Name="event_collectiondate")]

    public class Event : ReactiveObject
    {
        [Column(IsPrimaryKey=true)]
        public int EventID { get; set; }

        [Column]        
        public int SeriesID { get; set; }

        [Column]
        public DateTime CollectionDate { get; set; }

        [Column]
        public string LocalityDescription { get; set; }

        [Column]
        public string HabitatDescription { get; set; }

        [Column]
        public bool? IsModified { get; set; }

        public Event()
        {
            CollectionDate = DateTime.Now;
            IsModified = null;
        }
    }
}
