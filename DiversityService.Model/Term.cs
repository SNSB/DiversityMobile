using System;
using System.Net;


namespace DiversityService.Model
{    
    public class Term
    {
        //Read-Only

        public string Code { get; set; }
        public int SourceID { get; set; }
        public string Description { get; set; }
        public string DisplayText { get; set; }
        public string ParentCode { get; set; }
        public DateTime LogUpdatedWhen { get; set; }
    }
}
