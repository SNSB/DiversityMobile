using System;


namespace DiversityService.Model
{
    public class CollectionEventProperty
    {
        public int EventID{get;set;}
        public int? DiversityCollectionEventID{get;set;}
        public int PropertyID { get; set; }
        public String DisplayText { get; set; }
        public String PropertyUri { get; set; }

        public DateTime LogUpdatedWhen { get; set; }
    }
}
