using System;
using System.Net;


namespace DiversityService.Model
{
    public class TermList
    {
        public TermList(int id, string txt)
        {
            ListID = id;
            DisplayText = txt;
        }
        public TermList()
        {

        }

        public int ListID { get; set; }
        public string DisplayText { get; set; }
    }
}
