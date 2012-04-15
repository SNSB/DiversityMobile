using System;


namespace DiversityService.Model
{
    public class Property
    {
        //Read-Only

        public int PropertyID { get; set; }
        public string PropertyName { get; set; }
        public string DisplayText { get; set; }
        public string Description { get; set; }        
    }
}
