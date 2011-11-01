using System;

namespace DiversityService.Model
{
    public class Specimen
    {
        public int CollectionSpecimenID{get; set;}
        public int CollectionEventID { get; set; }
        public string AccesionNumber { get; set; }


        public DateTime LogUpdatedWhen { get; set; }
    }
}
