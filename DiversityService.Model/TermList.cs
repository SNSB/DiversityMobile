using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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
