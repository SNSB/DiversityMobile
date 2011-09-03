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
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq.Mapping;

namespace DiversityPhone.Model
{    
    [Table]
    [Index(Columns="LastUsed",IsUnique=false,Name="term_lastusage")]
    [Index(Columns = "ParentCode", IsUnique = false, Name = "term_inheritance")]
    public class Term
    {

        public static readonly DateTime DefaultLastUsed = new DateTime(2009, 01, 01); // DateTime.MinValue creates an overflow on insert.


        public Term()
        {
            ParentCode = null;
            LastUsed = DefaultLastUsed;
        }


        [Column(IsPrimaryKey=true)]
        public string Code { get; set; }

        [Column(IsPrimaryKey=true)]
        public int SourceID { get; set; } 

        [Column]
        public string Description { get; set; }

        [Column]
        public string DisplayText { get; set; }

        [Column]
        public string ParentCode { get; set; }

        [Column]
        public DateTime LastUsed {get; set;}
    }
}
