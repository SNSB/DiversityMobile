using System;
using System.Net;


namespace DiversityService.Model
{
    public class TaxonList
    {
        public TaxonList(int id, string txt)
        {
            ListID = id;
            DisplayText = txt;
        }
        public TaxonList()
        {

        }

        public int ListID { get; set; }
        public string DisplayText { get; set; }
    }
}
