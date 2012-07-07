using System;
using PetaPoco;


namespace DiversityService.Model
{
    public class CollectionEventProperty
    {
        [Ignore]
        public int EventID{get;set;}
        [Column("CollectionEventID")]
        public int? DiversityCollectionEventID{get;set;}
        public int PropertyID { get; set; }
        public String DisplayText { get; set; }
        public String PropertyUri { get; set; }

        public DateTime LogUpdatedWhen { get; set; }
    }
}
