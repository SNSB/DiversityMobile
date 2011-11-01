using System;


namespace DiversityService.Model
{
    public class MultimediaObject
    {
        public int SourceId { get; set; }
        public int RelatedId { get; set; }
        public String Uri {get;set;}
        public String MediaType { get; set; }
        public DateTime LogUpdatedWhen { get; set; }
    }
}
