using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace DiversityService.Model
{
    public class CollectionEventSeriesImage
    {
        public int SeriesID { get; set; }
        public string Uri{ get; set; }
        public string ImageType { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        [Ignore]
        public DateTime LogUpdatedWhen { get; set; }
    }
}
